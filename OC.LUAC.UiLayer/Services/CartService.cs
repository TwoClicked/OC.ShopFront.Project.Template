using Blazored.LocalStorage;
using OC.LUAC.UiLayer.DTO.Cart;

namespace OC.LUAC.UiLayer.Services
{
    public class CartService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly List<CartItemDto> _items = new();
        private const string StorageKey = "cart";

        public IReadOnlyList<CartItemDto> Items => _items;
        public event Action? OnChange;

        public CartService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task InitializeAsync()
        {
            var saved = await _localStorage.GetItemAsync<List<CartItemDto>>(StorageKey);
            if (saved != null)
            {
                _items.Clear();
                _items.AddRange(saved);
            }
        }

        private async Task Persist()
        {
            await _localStorage.SetItemAsync(StorageKey, _items);
            OnChange?.Invoke();
        }

        // ------------------------------
        // CART ACTIONS
        // ------------------------------

        public async Task<bool> AddOrIncrease(CartItemDto item)
        {
            var existing = _items.FirstOrDefault(x =>
                x.ProductId == item.ProductId && x.ProductVariantId == item.ProductVariantId);

            if (existing != null)
            {
                if (existing.Quantity + item.Quantity > item.MaxStock)
                    return false;

                existing.Quantity += item.Quantity;
            }
            else
            {
                if (item.Quantity > item.MaxStock)
                    return false;

                _items.Add(item);
            }

            await Persist();
            return true;
        }

        public async Task UpdateQuantity(int productId, int productVariantId, int quantity)
        {
            var existing = _items.FirstOrDefault(x =>
                x.ProductId == productId && x.ProductVariantId == productVariantId);

            if (existing != null)
            {
                if (quantity <= 0)
                    _items.Remove(existing);
                else
                    existing.Quantity = quantity;
            }
            await Persist();
        }

        public async Task RemoveFromCart(int productId, int productVariantId)
        {
            var existing = _items.FirstOrDefault(x =>
                x.ProductId == productId && x.ProductVariantId == productVariantId);

            if (existing != null)
                _items.Remove(existing);

            await Persist();
        }

        public async Task ClearCart()
        {
            _items.Clear();
            await Persist();
        }

        public decimal GetTotal() => _items.Sum(x => x.Total);
    }
}
