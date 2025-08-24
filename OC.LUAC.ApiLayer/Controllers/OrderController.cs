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
        private readonly IVoucherService _vouchers;
        private readonly IShippingZoneService _shippingZones;

        public OrderController(
            IOrderService orders,
            IStockActionService stock,
            ICustomerService customers,
            IProductService products,
            IProductVariantService variants,
            IEmailService emailService,
            IVoucherService voucherService,
            IShippingZoneService shippingService)
        {
            _orders = orders;
            _stock = stock;
            _customers = customers;
            _products = products;
            _variants = variants;
            _emailService = emailService;
            _vouchers = voucherService;
            _shippingZones = shippingService;

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

            // ---- calculate product totals ----
            decimal totalBeforeDiscount = builtItems.Sum(i => i.UnitPrice * i.Quantity);
            decimal discount = 0;
            decimal totalAfterDiscount = totalBeforeDiscount;

            // ---- apply voucher if provided ----
            if (!string.IsNullOrWhiteSpace(dto.VoucherCode))
            {
                var voucher = await _vouchers.GetVoucherByCodeAsync(dto.VoucherCode);
                if (voucher == null || !voucher.IsActive)
                    return BadRequest("Invalid or inactive voucher code.");

                if (DateTime.UtcNow < voucher.StartDate || DateTime.UtcNow > voucher.EndDate)
                    return BadRequest("Voucher not valid at this time.");

                if (voucher.MaxUsageCount.HasValue && voucher.CurrentUsageCount >= voucher.MaxUsageCount.Value)
                    return BadRequest("Voucher usage limit reached.");

                if (voucher.Percentage.HasValue)
                    discount += totalBeforeDiscount * (voucher.Percentage.Value / 100m);

                if (voucher.FixedAmount.HasValue)
                    discount += voucher.FixedAmount.Value;

                if (discount > totalBeforeDiscount) discount = totalBeforeDiscount;

                totalAfterDiscount = totalBeforeDiscount - discount;

                voucher.CurrentUsageCount++;
                await _vouchers.UpdateVoucherAsync(voucher);
            }

            // ---- calculate shipping ----
            var zone = await _shippingZones.GetZoneByCountryAsync(dto.ShippingCountry);
            if (zone == null)
                return BadRequest("We do not currently ship to this region.");

            decimal shippingCost = zone.BaseCost;
            bool isFreeShipping = false;

            if (totalBeforeDiscount >= zone.FreeShippingThreshold)
            {
                shippingCost = 0;
                isFreeShipping = true;
            }

            // Voucher override for free shipping
            if (!string.IsNullOrWhiteSpace(dto.VoucherCode))
            {
                var voucher = await _vouchers.GetVoucherByCodeAsync(dto.VoucherCode);
                if (voucher?.AppliesToShipping == true)
                {
                    shippingCost = 0;
                    isFreeShipping = true;
                }
            }

            totalAfterDiscount += shippingCost;

            // ---- build order ----
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
                Items = builtItems,
                VoucherCode = dto.VoucherCode,
                TotalBeforeDiscount = totalBeforeDiscount,
                DiscountAmount = discount,
                TotalAfterDiscount = totalAfterDiscount,
                ShippingCost = shippingCost,
                IsFreeShipping = isFreeShipping
            };

            var created = await _orders.CreateOrderAsync(order);

            // ---- generate PDF ----
            var pdf = PdfGenerator.GenerateOrderPdf(created);

            // fetch customer entity for email
            var customer = await _customers.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return BadRequest("Customer not found after order creation.");

            // ---- send confirmation email ----
            var lang = created.Language ?? "en";
            var t = Localization.T;

            var subject = $"{t(lang, "OrderConfirmation")} - {created.OrderNumber}";

            var body = $@"
             <p>{t(lang, "Hello")} {customer.FirstName},</p>
             <p>{t(lang, "ThanksForOrder")} <b>{created.OrderNumber}</b>.</p>

             <p><strong>{t(lang, "OrderNumber")}:</strong> {created.OrderNumber}</p>
             <p><strong>{t(lang, "Date")}:</strong> {created.CreatedAt:yyyy-MM-dd}</p>

             <table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse;'>
                <tr>
            <th>{t(lang, "Product")}</th>
            <th>{t(lang, "Qty")}</th>
            <th>{t(lang, "Price")}</th>
            <th>{t(lang, "Total")}</th>
             </tr>";

            foreach (var item in created.Items)
            {
                var lineTotal = item.Quantity * item.UnitPrice;
                body += $@"
             <tr>
                 <td>{item.ProductName} ({item.Size})</td>
                 <td>{item.Quantity}</td>
                 <td>{item.UnitPrice:C}</td>
                 <td>{lineTotal:C}</td>
             </tr>";
            }

            body += "</table>";

            // Subtotal + discount only if discount applied
            if (created.DiscountAmount.HasValue && created.DiscountAmount.Value > 0)
            {
                body += $@"<p><strong>{t(lang, "Subtotal")}:</strong> {created.TotalBeforeDiscount:C}</p>";
                body += $@"<p><strong>{t(lang, "Discount")} ({created.VoucherCode}):</strong> -{created.DiscountAmount:C}</p>";
            }

            // Shipping line (always show, even if free)
            body += $@"<p><strong>{t(lang, "Shipping")}:</strong> {(created.ShippingCost > 0 ? created.ShippingCost.ToString("C") : t(lang, "Free"))}</p>";

            body += $@"
            <p><strong>{t(lang, "GrandTotal")}:</strong> {created.TotalAfterDiscount:C}</p>
            <p>{t(lang, "ThankYou")}</p>";

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: subject,
                body: body,
                pdfAttachment: pdf,
                attachmentName: $"Order {created.OrderNumber}.pdf"
            );

            // record stock movement
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
            var lang = order.Language ?? "en";
            var subject = $"{Localization.T(lang, "OrderShipped")} - {order.OrderNumber}";

            var trackingText = (!string.IsNullOrEmpty(order.TrackingNumber) && !string.IsNullOrEmpty(order.TrackingUrl))
                ? $"<p>{Localization.T(lang, "OrderShipped")} <br/>" +
                  $"{Localization.T(lang, "TrackingNumber")}: <b>{order.TrackingNumber}</b><br/>" +
                  $"Track here: <a href='{order.TrackingUrl}' target='_blank'>{order.TrackingUrl}</a></p>"
                : !string.IsNullOrEmpty(order.TrackingNumber)
                    ? $"<p>{Localization.T(lang, "TrackingNumber")}: <b>{order.TrackingNumber}</b></p>"
                    : "<p>No tracking information provided.</p>";

            var body = $@"
             <p>{Localization.T(lang, "Hello")} {customer.FirstName},</p>
             {trackingText}
             <p>{Localization.T(lang, "ThankYou")}</p>";


            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: subject,
                body: body);

            return Ok(new { id, status = "Shipped" });

        }

        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = await _orders.CancelOrderAsync(id);
            if (!ok) return NotFound();

            var order = await _orders.GetOrderByIdAsync(id, includeDeleted: true); 
            if (order == null) return NotFound("Order not found after cancellation.");

            var customer = await _customers.GetCustomerByIdAsync(order.CustomerId.Value);
            if (customer == null) return NotFound("Customer not found.");

            var lang = order.Language ?? "en";
            var subject = $"{Localization.T(lang, "OrderCancelledSubject")} - {order.OrderNumber}";
            var body = $@"
             <p>{Localization.T(lang, "Hello")} {customer.FirstName},</p>
             <p>{Localization.T(lang, "OrderCancelledBody")}</p>
             <p>{Localization.T(lang, "ThankYou")}</p>";

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: subject,
                body: body
            );

            return Ok(new { id, status = "Cancelled" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("shipped")]
        public async Task<ActionResult<IEnumerable<Order>>> GetShippedOrders()
        {
            var orders = await _orders.GetShippedOrdersAsync();
            return Ok(orders);
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
