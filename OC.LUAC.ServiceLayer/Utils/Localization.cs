namespace OC.LUAC.ServiceLayer.Utils
{
    public static class Localization
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Translations =
            new()
            {
                ["en"] = new Dictionary<string, string>
                {
                    // =======================
                    // 🔹 ORDER FLOW
                    // =======================
                    { "OrderConfirmation", "Order Confirmation" },
                    { "OrderShipped", "Your order has shipped!" },
                    { "OrderCancelledSubject", "Your order has been cancelled" },

                    // =======================
                    // 🔹 GENERIC PHRASES
                    // =======================
                    { "Hello", "Hi" },
                    { "ThanksForOrder", "Thanks for your order" },
                    { "ThankYou", "Thank you for shopping with LUAC!" },
                    { "OrderNote", "This is your order confirmation. Please keep it for your records." },

                    // =======================
                    // 🔹 CUSTOMER + SHIPPING
                    // =======================
                    { "Customer", "Customer" },
                    { "ShippingAddress", "Shipping Address" },

                    // =======================
                    // 🔹 ORDER TABLE
                    // =======================
                    { "Product", "Product" },
                    { "Qty", "Qty" },
                    { "Price", "Price" },
                    { "Total", "Line Total" },
                    { "Subtotal", "Subtotal" },
                    { "Discount", "Discount" },
                    { "Shipping", "Flat-rate shipping" },
                    { "GrandTotal", "Grand Total" },

                    // =======================
                    // 🔹 ORDER META
                    // =======================
                    { "OrderNumber", "Order Number" },
                    { "Date", "Date" },
                    { "TrackingNumber", "Tracking Number" },
                    { "TrackHere", "Track here" },

                    // =======================
                    // 🔹 PAYMENT INFO
                    // =======================
                    { "PaymentInformation", "Payment Information" },
                    { "PleaseTransfer", "Please transfer the total amount within 5 business days to the following bank account:" },
                    { "AccountHolder", "Account Holder" },
                    { "IBAN", "IBAN" },
                    { "BIC", "BIC" },
                    { "Bank", "Bank" },
                    { "Reference", "Reference" },
                    { "ImportantNotice", "Important Notice" },
                    { "OrderProcessedAfterPayment", "Your order will only be processed and shipped after the full payment has been received." },
                    { "OrderCancelledIfNoPayment", "If we do not receive your payment within 5 business days, the order will be cancelled automatically." },

                    // =======================
                    // 🔹 PAYMENT STATUS
                    // =======================
                    { "PaymentReceived", "Payment Received" },
                    { "PaymentReceivedMessage", "We have received your payment. Your order is now being processed and will be shipped soon." },

                    // =======================
                    // 🔹 CANCELLATION
                    // =======================
                    { "OrderCancelledBody", "We confirm that your order has been cancelled." },
                    { "OrderCancelledByCustomerBody", "You have cancelled your order." },
                    { "OrderCancelledNoPaymentBody", "We did not receive your payment within 5 business days, so your order has been cancelled." }
                },

                ["de"] = new Dictionary<string, string>
                {
                    // =======================
                    // 🔹 ORDER FLOW
                    // =======================
                    { "OrderConfirmation", "Bestellbestätigung" },
                    { "OrderShipped", "Ihre Bestellung wurde versendet!" },
                    { "OrderCancelledSubject", "Ihre Bestellung wurde storniert" },

                    // =======================
                    // 🔹 GENERIC PHRASES
                    // =======================
                    { "Hello", "Hallo" },
                    { "ThanksForOrder", "Vielen Dank für Ihre Bestellung" },
                    { "ThankYou", "Vielen Dank für Ihren Einkauf bei LUAC!" },
                    { "OrderNote", "Dies ist Ihre Bestellbestätigung. Bitte bewahren Sie sie für Ihre Unterlagen auf." },

                    // =======================
                    // 🔹 CUSTOMER + SHIPPING
                    // =======================
                    { "Customer", "Kunde" },
                    { "ShippingAddress", "Lieferadresse" },

                    // =======================
                    // 🔹 ORDER TABLE
                    // =======================
                    { "Product", "Produkt" },
                    { "Qty", "Menge" },
                    { "Price", "Preis" },
                    { "Total", "Zwischensumme" },
                    { "Subtotal", "Zwischensumme" },
                    { "Discount", "Rabatt" },
                    { "Shipping", "Versandpauschale" },
                    { "Free", "Kostenlos" },
                    { "GrandTotal", "Gesamtsumme" },

                    // =======================
                    // 🔹 ORDER META
                    // =======================
                    { "OrderNumber", "Bestellnummer" },
                    { "Date", "Datum" },
                    { "TrackingNumber", "Sendungsverfolgungsnummer" },
                    { "TrackHere", "Sendung hier verfolgen" },

                    // =======================
                    // 🔹 PAYMENT INFO
                    // =======================
                    { "PaymentInformation", "Zahlungsinformationen" },
                    { "PleaseTransfer", "Bitte überweisen Sie den Rechnungsbetrag innerhalb von 5 Werktagen auf folgendes Konto:" },
                    { "AccountHolder", "Kontoinhaber" },
                    { "IBAN", "IBAN" },
                    { "BIC", "BIC" },
                    { "Bank", "Bank" },
                    { "Reference", "Verwendungszweck" },
                    { "ImportantNotice", "Wichtiger Hinweis" },
                    { "OrderProcessedAfterPayment", "Ihre Bestellung wird erst nach vollständigem Zahlungseingang bearbeitet und versendet." },
                    { "OrderCancelledIfNoPayment", "Sollte keine Zahlung innerhalb von 5 Werktagen eingehen, behalten wir uns vor, die Bestellung zu stornieren." },

                    // =======================
                    // 🔹 PAYMENT STATUS
                    // =======================
                    { "PaymentReceived", "Zahlung erhalten" },
                    { "PaymentReceivedMessage", "Wir haben Ihre Zahlung erhalten. Ihre Bestellung wird nun bearbeitet und in Kürze versendet." },

                    // =======================
                    // 🔹 CANCELLATION
                    // =======================
                    { "OrderCancelledBody", "Wir bestätigen hiermit, dass Ihre Bestellung storniert wurde." },
                    { "OrderCancelledByCustomerBody", "Sie haben Ihre Bestellung storniert." },
                    { "OrderCancelledNoPaymentBody", "Wir haben Ihre Zahlung nicht innerhalb von 5 Werktagen erhalten, daher wurde Ihre Bestellung storniert." }
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
