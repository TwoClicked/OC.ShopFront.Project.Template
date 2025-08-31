// OC.LUAC.ApiLayer/Controllers/OrderController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Order;
using OC.LUAC.ApiLayer.DTO.Product;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;
using OC.LUAC.ServiceLayer.Utils;
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

            // ---- determine customer (existing by email OR create guest) ----
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

                var existingByEmail = await _customers.GetCustomerByEmailAsync(dto.Email.Trim());
                if (existingByEmail != null)
                {
                    customerId = existingByEmail.Id;
                }
                else
                {
                    var guest = new Customer
                    {
                        Email = dto.Email.Trim(),
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Language = dto.Language ?? "en",
                        CreatedAt = DateTime.UtcNow
                    };

                    var randomPassword = Guid.NewGuid().ToString("N");
                    guest = await _customers.RegisterAsync(guest, randomPassword)
                            ?? throw new InvalidOperationException("Guest registration failed.");
                    customerId = guest.Id;
                }
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

            // Voucher override for free shipping (if supported on your Voucher)
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
                Language = dto.Language ?? "en",
                ShippingStreet = dto.ShippingStreet,
                ShippingNumber = dto.ShippingNumber,
                ShippingPostalCode = dto.ShippingPostalCode,
                ShippingCity = dto.ShippingCity,
                ShippingCountry = dto.ShippingCountry,
                Status = OrderStatus.PendingPayment,
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

            // ---- generate PDF & send confirmation email ----
            var pdf = PdfGenerator.GenerateOrderPdf(created);

            var customer = await _customers.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return BadRequest("Customer not found after order creation.");

            var lang = created.Language ?? "en";
            var t = Localization.T;

            var subject = $"{t(lang, "OrderConfirmation")} - {created.OrderNumber}";

            var body = $@"
            <p>{t(lang, "Hello")} {customer.FirstName},</p>
            <p>{t(lang, "ThanksForOrder")}</p>

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

            if (created.DiscountAmount.HasValue && created.DiscountAmount.Value > 0)
            {
                body += $@"<p><strong>{t(lang, "Subtotal")}:</strong> {created.TotalBeforeDiscount:C}</p>";
                body += $@"<p><strong>{t(lang, "Discount")} ({created.VoucherCode}):</strong> -{created.DiscountAmount:C}</p>";
            }

            body += $@"<p><strong>{t(lang, "GrandTotal")}:</strong> {created.TotalAfterDiscount:C}</p>";

            body += $@"
            <p><strong>{t(lang, "ImportantNotice")}:</strong></p>
            <p>{t(lang, "OrderProcessedAfterPayment")}</p>
            <p>{t(lang, "OrderCancelledIfNoPayment")}</p>
            <p>{t(lang, "ThankYou")}</p>";

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: subject,
                body: body,
                pdfAttachment: pdf,
                attachmentName: $"Order {created.OrderNumber}.pdf"
            );

            // ---- reduce stock ----
            foreach (var oi in created.Items)
            {
                await _stock.RecordStockChangeAsync(
                    oi.ProductVariantId,
                    oi.Quantity,
                    StockActionType.Sold,
                    created.Id);
            }
            var response = new OrderResponseDto
            {
                OrderId = created.OrderNumber,  
                Message = "Order created successfully"
            };

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);

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

            // Fetch order + customer for email
            var order = await _orders.GetOrderByIdAsync(id);
            if (order == null) return NotFound("Order not found after shipping update");

            var customer = await _customers.GetCustomerByIdAsync(order.CustomerId.Value);
            if (customer == null) return NotFound("Customer not found for this order.");

            var lang = order.Language ?? "en";
            var subject = $"{Localization.T(lang, "OrderShipped")} - {order.OrderNumber}";

            var trackingText =
                (!string.IsNullOrEmpty(order.TrackingNumber) && !string.IsNullOrEmpty(order.TrackingUrl))
                ? $"<p>{Localization.T(lang, "OrderShipped")}<br/>" +
                  $"{Localization.T(lang, "TrackingNumber")}: <b>{order.TrackingNumber}</b><br/>" +
                  $"{Localization.T(lang, "TrackHere")}: <a href='{order.TrackingUrl}' target='_blank'>{order.TrackingUrl}</a></p>"
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

        // Customer self-cancel
        [HttpPut("{id:int}/cancel-me")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelMyOrder(int id)
        {
            var idClaim = User.FindFirst("customerId")
                         ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (idClaim == null || !int.TryParse(idClaim.Value, out var customerId))
                return Unauthorized("No valid customer ID found in token.");

            var order = await _orders.GetOrderByIdAsync(id);
            if (order == null || order.CustomerId != customerId)
                return NotFound("Order not found or not yours.");

            if (order.Status == OrderStatus.Shipped)
                return BadRequest("You cannot cancel an order that has already been shipped.");

            var ok = await _orders.CancelOrderAsync(id);
            if (!ok) return BadRequest("Cancellation failed.");

            var customer = await _customers.GetCustomerByIdAsync(order.CustomerId.Value);
            if (customer == null)
                return NotFound("Customer not found.");

            var lang = order.Language ?? "en";
            var subject = $"{Localization.T(lang, "OrderCancelledSubject")} - {order.OrderNumber}";
            var body = $@"
            <p>{Localization.T(lang, "Hello")} {customer.FirstName},</p>
            <p>{Localization.T(lang, "OrderCancelledByCustomerBody")}</p>
            <p>{Localization.T(lang, "ThankYou")}</p>";

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: subject,
                body: body
            );

            return Ok(new { id, status = "Cancelled by Customer" });
        }

        // Admin: cancel for no payment
        [HttpPut("{id:int}/cancel-nopayment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelForNoPayment(int id)
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
            <p>{Localization.T(lang, "OrderCancelledNoPaymentBody")}</p>
            <p>{Localization.T(lang, "ThankYou")}</p>";

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: subject,
                body: body
            );

            return Ok(new { id, status = "Cancelled (No Payment)" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("shipped")]
        public async Task<ActionResult<IEnumerable<Order>>> GetShippedOrders()
        {
            var orders = await _orders.GetShippedOrdersAsync();
            return Ok(orders);
        }

        // Mark Paid
        [HttpPut("{id:int}/mark-paid")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var order = await _orders.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            if (order.Status != OrderStatus.PendingPayment)
                return BadRequest("Only pending payment orders can be marked as paid.");

            order.Status = OrderStatus.Processing; // payment confirmed by admin goes into processing
            await _orders.UpdateOrderAsync(order);

            // Send confirmation email to customer
            var customer = await _customers.GetCustomerByIdAsync(order.CustomerId.Value);
            if (customer == null) return NotFound("Customer not found.");

            var lang = order.Language ?? "en";
            var t = Localization.T;

            var subject = $"{t(lang, "PaymentReceived")} - {order.OrderNumber}";
            var body = $@"
                <p>{t(lang, "Hello")} {customer.FirstName},</p>
                <p>{t(lang, "PaymentReceivedMessage")}</p>
                <p><strong>{t(lang, "OrderNumber")}:</strong> {order.OrderNumber}</p>
                <p>{t(lang, "ThankYou")}</p>";

            await _emailService.SendEmailAsync(
                to: customer.Email,
                subject: subject,
                body: body
            );

            return Ok(new { id = order.Id, status = "Paid" });
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
