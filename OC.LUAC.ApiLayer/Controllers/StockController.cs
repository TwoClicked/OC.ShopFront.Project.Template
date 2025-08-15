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

        public StockController(IStockActionService stock, IOrderService order)
        {
            _stock = stock;
            _order = order;
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
    }
}
