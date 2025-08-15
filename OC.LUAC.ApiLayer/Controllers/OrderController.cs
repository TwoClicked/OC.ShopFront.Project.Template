// OC.LUAC.ApiLayer/Controllers/OrderController.cs
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Order;
using OC.LUAC.ObjectLayer.Accounts;   // Customer
using OC.LUAC.ObjectLayer.Entities;   // StockActionType
using OC.LUAC.ObjectLayer.Orders;     // Order, OrderItem, OrderStatus
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public OrderController(
            IOrderService orders,
            IStockActionService stock,
            ICustomerService customers,
            IProductService products,
            IProductVariantService variants)
        {
            _orders = orders;
            _stock = stock;
            _customers = customers;
            _products = products;
            _variants = variants;
        }

        // GET /api/orders/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Order), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Order>> GetById(int id)
        {
            var order = await _orders.GetOrderByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        // GET /api/orders/customer/{customerId}
        [HttpGet("customer/{customerId:int}")]
        [ProducesResponseType(typeof(List<Order>), 200)]
        public async Task<ActionResult<List<Order>>> GetForCustomer(int customerId)
        {
            var list = await _orders.GetOrdersByCustomerIdAsync(customerId);
            return Ok(list);
        }

        // POST /api/orders  (guest OR existing customer)
        [HttpPost]
        [ProducesResponseType(typeof(Order), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Order>> Create([FromBody] CreateOrderDto dto)
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

                // register guest with random password
                var randomPassword = Guid.NewGuid().ToString("N");
                guest = await _customers.RegisterAsync(guest, randomPassword)
                        ?? throw new InvalidOperationException("Guest registration failed.");
                customerId = guest.Id;
            }

            // ---- build items with snapshots & validate stock ahead of time ----
            var builtItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                // product
                var product = await _products.GetProductByIdAsync(item.ProductId);
                if (product == null)
                    return BadRequest($"Product {item.ProductId} not found.");

                // variant (must belong to product)
                var variantsForProduct = await _variants.GetVariantsByProductIdAsync(item.ProductId);
                var variant = variantsForProduct.FirstOrDefault(v => v.Id == item.ProductVariantId);
                if (variant == null)
                    return BadRequest($"Variant {item.ProductVariantId} not found for product {item.ProductId}.");

                // stock check before we create the order
                if (variant.Stock < item.Quantity)
                    return BadRequest($"Insufficient stock for variant {variant.Id}. Available: {variant.Stock}, requested: {item.Quantity}.");

                // snapshots (prefer client-sent values if present)
                var snapName = string.IsNullOrWhiteSpace(item.ProductName) ? product.Name_en : item.ProductName!;
                var snapSize = string.IsNullOrWhiteSpace(item.Size) ? variant.Size : item.Size!;
                var snapPrice = item.UnitPrice ?? product.Price;

                builtItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    productVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    UnitPrice = snapPrice,
                    ProductName = snapName,
                    Size = snapSize
                });
            }

            // ---- create order ----
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

            // ---- adjust stock (Sold) ----
            foreach (var oi in created.Items)
            {
                await _stock.RecordStockChangeAsync(
                    oi.productVariantId,
                    oi.Quantity,
                    StockActionType.Sold,
                    created.Id);
            }

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/orders/{id}/ship
        [HttpPut("{id:int}/ship")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkShipped(int id, [FromBody] ShipOrderDto dto)
        {
            // No Status required here; we always set to Shipped via the service
            var ok = await _orders.MarkOrderAsShippedAsync(
                id,
                dto?.TrackingNumber ?? string.Empty,
                dto?.TrackingUrl ?? string.Empty
            );

            return ok ? Ok(new { id, status = "Shipped" }) : NotFound();
        }


        // PUT /api/orders/{id}/cancel   (restocks items)
        [HttpPut("{id:int}/cancel")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = await _orders.CancelOrderAsync(id);
            if (!ok) return NotFound();

            var order = await _orders.GetOrderByIdAsync(id);
            if (order != null)
            {
                foreach (var oi in order.Items)
                {
                    await _stock.RecordStockChangeAsync(
                        oi.productVariantId,
                        oi.Quantity,
                        StockActionType.CancelledRestock,
                        order.Id);
                }
            }

            return Ok(new { id, status = "Cancelled" });
        }

        // --- helper ---
        private static string GenerateOrderNumber()
            => $"{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}
