using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Stock;
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ObjectLayer.Orders;
using Microsoft.AspNetCore.Authorization;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly IStockActionService _stock;
        private readonly IOrderService _order;
        private readonly IProductVariantService _variants;

        public StockController(IStockActionService stock, IOrderService order, IProductVariantService variants)
        {
            _stock = stock;
            _order = order;
            _variants = variants;
        }

        // GET /api/stock/variant/{variantId}/log
        [HttpGet("variant/{variantId:int}/log")]
        public async Task<IActionResult> GetLogForVariant(int variantId)
        {
            var log = await _stock.GetStockLogByVariantAsync(variantId);
            return Ok(log);
        }

        // GET /api/stock/recent?count=20
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 20)
        {
            var items = await _stock.GetRecentStockActionsAsync(count);
            return Ok(items);
        }

        // POST /api/stock/adjust
        [HttpPost("adjust")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Adjust([FromBody] AdjustStockDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (!Enum.TryParse<StockActionType>(dto.ActionType, true, out var actionType))
                return BadRequest("Invalid ActionType.");

            if (dto.OrderId.HasValue)
            {
                var order = await _order.GetOrderByIdAsync(dto.OrderId.Value);
                if (order == null) return BadRequest($"Order {dto.OrderId.Value} does not exist.");
            }

            var action = await _stock.RecordStockChangeAsync(dto.VariantId, dto.QuantityChange, actionType, dto.OrderId);
            if (action == null) return Problem("Stock adjustment failed.");

            return Ok(action);
        }

        // GET /api/stock/variant/{variantId}
        [HttpGet("variant/{variantId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> GetStockForVariant(int variantId)
        {
            var variant = await _variants.GetVariantByIdAsync(variantId);
            if (variant == null)
                return NotFound();

            return Ok(variant.Stock);
        }

        // GET /api/stock/product/{productId}
        [HttpGet("product/{productId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<object>>> GetStockForProduct(int productId)
        {
            var list = await _variants.GetVariantsByProductIdAsync(productId);
            if (list == null || list.Count == 0)
                return NotFound();

            return Ok(list.Select(v => new
            {
                v.Id,
                v.Size,
                v.Stock
            }));
        }

        [HttpGet("low")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<object>>> GetLowStock([FromQuery] int threshold = 5)
        {
            var all = await _variants.GetAllVariantsAsync();

            var low = all
                .Where(v => v.Stock <= threshold)
                .Select(v => new
                {
                    v.Id,
                    v.ProductId,
                    NameEn = v.Product?.Name_en ?? "(unknown)",
                    NameDe = v.Product?.Name_de ?? "(unknown)",
                    v.Size,
                    v.Stock
                });

            return Ok(low);
        }


    }
}
