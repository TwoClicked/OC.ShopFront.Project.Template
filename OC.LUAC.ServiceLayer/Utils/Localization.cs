namespace OC.LUAC.ServiceLayer.Utils
{
    public static class Localization
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Translations =
            new()
            {
                ["en"] = new Dictionary<string, string>
                {
                    { "OrderConfirmation", "Order Confirmation" },
                    { "OrderShipped", "Your order has shipped!" },
                    { "Hello", "Hi" },
                    { "ThanksForOrder", "Thanks for your order" },
                    { "ShippingAddress", "Shipping Address" },
                    { "Customer", "Customer" },
                    { "Product", "Product" },
                    { "Qty", "Qty" },
                    { "Price", "Price" },
                    { "Total", "Line Total" },
                    { "GrandTotal", "Grand Total" },
                    { "ThankYou", "Thank you for shopping with LUAC!" },
                    { "OrderNote", "This is your order confirmation. Please keep it for your records." },
                    { "Date", "Date" }, 
                    { "OrderNumber", "Order Number" },
                    { "TrackingNumber", "Tracking Number"},
                    { "OrderCancelledSubject", "Your order has been cancelled" },
                    { "OrderCancelledBody", "We’re writing to confirm that your order has been cancelled." },
                    { "Subtotal", "Subtotal" },
                    { "Discount", "Discount"},
                    { "Shipping", "Shipping"},
                    { "Free", "Free"}


                },
                ["de"] = new Dictionary<string, string>
                {
                    { "OrderConfirmation", "Bestellbestätigung" },
                    { "OrderShipped", "Ihre Bestellung wurde versendet!" },
                    { "Hello", "Hallo" },
                    { "ThanksForOrder", "Vielen Dank für Ihre Bestellung" },
                    { "ShippingAddress", "Lieferadresse" },
                    { "Customer", "Kunde" },
                    { "Product", "Produkt" },
                    { "Qty", "Menge" },
                    { "Price", "Preis" },
                    { "Total", "Zwischensumme" },
                    { "GrandTotal", "Gesamtsumme" },
                    { "ThankYou", "Vielen Dank für Ihren Einkauf bei LUAC!" },
                    { "OrderNote", "Dies ist Ihre Bestellbestätigung. Bitte bewahren Sie sie für Ihre Unterlagen auf." },
                    { "Date", "Datum" },
                    { "OrderNumber", "Bestellnummer" },
                    { "TrackingNumber", "Sendungsverfolgungsnummer"},
                    { "OrderCancelledSubject", "Ihre Bestellung wurde storniert" },
                    { "OrderCancelledBody", "Wir bestätigen hiermit, dass Ihre Bestellung storniert wurde." },
                    { "Subtotal", "Zwischensumme" },
                    { "Discount", "Rabatt"},
                    { "Shipping", "Versand"},
                    { "Free", "Kostenlos"}
                }
            };

        public static string T(string lang, string key)
        {
            lang = string.IsNullOrWhiteSpace(lang) ? "en" : lang.ToLower();
            if (!Translations.ContainsKey(lang)) lang = "en";
            return Translations[lang].TryGetValue(key, out var val) ? val : key;
        }
    }
}
