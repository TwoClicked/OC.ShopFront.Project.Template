using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Order;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;
using OC.LUAC.ServiceLayer.Utils;
using System.Diagnostics.Metrics;
using System.Security.Claims;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orders;
        private readonly IStockActionService _stock;
        private readonly ICustomerService _customers;
        private readonly IProductService _products;
        private readonly IProductVariantService _variants;
        private readonly IEmailService _emailService;

        public OrderController(
            IOrderService orders,
            IStockActionService stock,
            ICustomerService customers,
            IProductService products,
            IProductVariantService variants,
            IEmailService emailService)
        {
            _orders = orders;
            _stock = stock;
            _customers = customers;
            _products = products;
            _variants = variants;
            _emailService = emailService;
        }

        // -------------------------------------------------
        // READ ENDPOINTS
        // -------------------------------------------------

        // GET /api/orders/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(OrderSummaryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<OrderSummaryDto>> GetById(int id)
        {
            var order = await _orders.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            return Ok(MapToDetailDto(order));
        }

        // GET /api/orders/all  (ADMIN - list all orders)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 200) pageSize = 200;

            var allOrders = await _orders.GetAllOrdersAsync();
            var total = allOrders.Count();

            var items = allOrders
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToSummaryDto)
                .ToList();

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            });
        }

        // GET /api/orders/my  (CUSTOMER - own orders)
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetMyOrders()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return Unauthorized("No customer ID in token");

            if (!int.TryParse(claim.Value, out var customerId))
                return Unauthorized("Invalid customer ID in token");

            var list = await _orders.GetOrdersByCustomerIdAsync(customerId);
            return Ok(list.OrderByDescending(o => o.CreatedAt).Select(MapToSummaryDto).ToList());
        }

        // GET /api/orders/customer/{customerId} (admin/debug)
        [HttpGet("customer/{customerId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<OrderSummaryDto>), 200)]
        public async Task<ActionResult<List<OrderSummaryDto>>> GetForCustomer(int customerId)
        {
            var list = await _orders.GetOrdersByCustomerIdAsync(customerId);
            return Ok(list.Select(MapToSummaryDto).ToList());
        }

        // -------------------------------------------------
        // WRITE ENDPOINTS
        // -------------------------------------------------

        [HttpPost]
        [ProducesResponseType(typeof(OrderSummaryDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<OrderSummaryDto>> Create([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // ---- determine customer (existing or guest) ----
            int customerId;
            if (dto.CustomerId.HasValue)
            {
                var existing = await _customers.GetCustomerByIdAsync(dto.CustomerId.Value);
                if (existing == null) return BadRequest($"Customer {dto.CustomerId.Value} does not exist.");
                customerId = existing.Id;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest("Email is required when CustomerId is not provided.");

                var guest = new Customer
                {
                    Email = dto.Email.Trim(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Language = dto.Language,
                    CreatedAt = DateTime.UtcNow
                };

                var randomPassword = Guid.NewGuid().ToString("N");
                guest = await _customers.RegisterAsync(guest, randomPassword)
                        ?? throw new InvalidOperationException("Guest registration failed.");
                customerId = guest.Id;
            }

            // ---- build items with snapshots & validate stock ----
            var builtItems = new List<OrderItem>();
            foreach (var item in dto.Items)
            {
                var product = await _products.GetProductByIdAsync(item.ProductId);
                if (product == null)
                    return BadRequest($"Product {item.ProductId} not found.");

                var variantsForProduct = await _variants.GetVariantsByProductIdAsync(item.ProductId);
                var variant = variantsForProduct.FirstOrDefault(v => v.Id == item.ProductVariantId);
                if (variant == null)
                    return BadRequest($"Variant {item.ProductVariantId} not found for product {item.ProductId}.");

                if (variant.Stock < item.Quantity)
                    return BadRequest($"Insufficient stock for variant {variant.Id}. Available: {variant.Stock}, requested: {item.Quantity}.");

                var snapName = string.IsNullOrWhiteSpace(item.ProductName) ? product.Name_en : item.ProductName!;
                var snapSize = string.IsNullOrWhiteSpace(item.Size) ? variant.Size : item.Size!;
                var snapPrice = item.UnitPrice ?? product.Price;

                builtItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    UnitPrice = snapPrice,
                    ProductName = snapName,
                    Size = snapSize
                });
            }

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CustomerId = customerId,
                Language = dto.Language,
                ShippingStreet = dto.ShippingStreet,
                ShippingNumber = dto.ShippingNumber,
                ShippingPostalCode = dto.ShippingPostalCode,
                ShippingCity = dto.ShippingCity,
                ShippingCountry = dto.ShippingCountry,
                Status = OrderStatus.New,
                CreatedAt = DateTime.UtcNow,
                Items = builtItems
            };

            var created = await _orders.CreateOrderAsync(order);

            //Generate PDF
            var pdf = PdfGenerator.GenerateOrderPdf(created);

            // fetch customer entity for email
            var customer = await _customers.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return BadRequest("Customer not found after order creation.");

            //Send confirmation email

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: $"Order Confirmation - {created.OrderNumber}",
                body: $"<p>Hi {customer.FirstName},</p><p>Thanks for your order <b>{created.OrderNumber}</b>!</p>",
                pdfAttachment: pdf,
                attachmentName: $"Order-{created.OrderNumber}.pdf"
            );


            foreach (var oi in created.Items)
            {
                await _stock.RecordStockChangeAsync(
                    oi.ProductVariantId,
                    oi.Quantity,
                    StockActionType.Sold,
                    created.Id);
            }

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDetailDto(created));
        }

        [HttpPut("{id:int}/ship")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkShipped(int id, [FromBody] ShipOrderDto dto)
        {
            var ok = await _orders.MarkOrderAsShippedAsync(
                id,
                dto?.TrackingNumber ?? string.Empty,
                dto?.TrackingUrl ?? string.Empty
            );

            if (!ok) return NotFound();

            //Fetch order with customer

            var order = await _orders.GetOrderByIdAsync(id);
            if (order == null) return NotFound("Order not found after shipping update");

            var customer = await _customers.GetCustomerByIdAsync(order.CustomerId.Value);
            if (customer == null) return NotFound("Customer not found for this order.");

            // build email body
            var trackingText = (!string.IsNullOrEmpty(order.TrackingNumber) && !string.IsNullOrEmpty(order.TrackingUrl))
                ? $"<p>Your tracking number: <b>{order.TrackingNumber}</b><br/>" +
                  $"Track here: <a href='{order.TrackingUrl}' target='_blank'>{order.TrackingUrl}</a></p>"
                : !string.IsNullOrEmpty(order.TrackingNumber)
                    ? $"<p>Your tracking number: <b>{order.TrackingNumber}</b></p>"
                    : "<p>No tracking information was provided.</p>";

            var body = $@"
             <p>Hi {customer.FirstName},</p>
             <p>Your order <b>{order.OrderNumber}</b> has been shipped!</p>
             {trackingText}
            <p>Thank you for shopping with LUAC.</p>";

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: $"Your order {order.OrderNumber} has shipped!",
                body: body);

            return Ok(new { id, status = "Shipped" });

        }

        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = await _orders.CancelOrderAsync(id);
            return ok ? Ok(new { id, status = "Cancelled" }) : NotFound();
        }


        // --- helpers ---
        private static string GenerateOrderNumber()
            => $"{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        private static OrderSummaryDto MapToSummaryDto(Order o) => new OrderSummaryDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt
        };

        private static OrderSummaryDto MapToDetailDto(Order o) => new OrderSummaryDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductName = i.ProductName,
                Size = i.Size,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}
