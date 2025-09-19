// OC.LUAC.ServiceLayer/Utils/PdfGenerator.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OC.LUAC.ObjectLayer.Orders;
using System.Globalization;

namespace OC.LUAC.ServiceLayer.Utils
{
    public static class PdfGenerator
    {
        public static byte[] GenerateOrderPdf(Order order, PaymentInformation paymentInfo)
        {
            var lang = order.Language ?? "en";
            var t = Localization.T;

            // force Euro formatting
            var euro = new CultureInfo("de-DE");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // ========== HEADER ==========
                    var logoPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "images",
                        "LUAC_DESIGN.png"
                    );

                    page.Header().Row(row =>
                    {
                        row.ConstantItem(200)
                            .Height(120)
                            .Image(logoPath, ImageScaling.FitArea);

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().AlignRight().Text(t(lang, "OrderConfirmation"))
                                .FontSize(16).SemiBold().FontColor(Colors.Black);

                            col.Item().AlignRight()
                                .Text($"{t(lang, "OrderNumber")}: {order.OrderNumber}")
                                .FontSize(11).FontColor(Colors.Grey.Darken2);

                            col.Item().AlignRight()
                                .Text($"{t(lang, "Date")}: {order.CreatedAt:yyyy-MM-dd}")
                                .FontSize(11).FontColor(Colors.Grey.Darken2);
                        });
                    });

                    // ========== CONTENT ==========
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // CUSTOMER + SHIPPING INFO
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t(lang, "Customer")).SemiBold();
                                c.Item().Text($"{order.Customer?.FirstName} {order.Customer?.LastName}");
                                c.Item().Text(order.Customer?.Email ?? "");
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t(lang, "ShippingAddress")).SemiBold();
                                c.Item().Text($"{order.ShippingStreet} {order.ShippingNumber}");
                                c.Item().Text($"{order.ShippingCountry} {order.ShippingPostalCode} {order.ShippingCity}");
                            });
                        });

                        col.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // ORDER ITEMS
                        col.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text(t(lang, "Product")).SemiBold();
                                header.Cell().Element(CellStyle).Text(t(lang, "Qty")).SemiBold();
                                header.Cell().Element(CellStyle).Text(t(lang, "Price")).SemiBold();
                                header.Cell().Element(CellStyle).Text(t(lang, "Total")).SemiBold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.PaddingVertical(5)
                                        .BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            foreach (var item in order.Items)
                            {
                                var lineTotal = item.UnitPrice * item.Quantity;
                                table.Cell().Element(Cell).Text($"{item.ProductName} ({item.Size})");
                                table.Cell().Element(Cell).Text(item.Quantity.ToString());
                                table.Cell().Element(Cell).Text(item.UnitPrice.ToString("C", euro));
                                table.Cell().Element(Cell).Text(lineTotal.ToString("C", euro));
                            }

                            // Subtotal + discount
                            if (order.DiscountAmount.HasValue && order.DiscountAmount.Value > 0)
                            {
                                table.Cell().ColumnSpan(3).Element(Cell).AlignRight().PaddingRight(10)
                                    .Text(t(lang, "Subtotal")).SemiBold();
                                table.Cell().Element(Cell).Text(order.TotalBeforeDiscount.ToString("C", euro)).SemiBold();

                                table.Cell().ColumnSpan(3).Element(Cell).AlignRight().PaddingRight(10)
                                    .Text($"{t(lang, "Discount")} ({order.VoucherCode})").SemiBold();
                                table.Cell().Element(Cell).Text($"-{order.DiscountAmount.Value.ToString("C", euro)}").SemiBold();
                            }

                            // Shipping (always show, even if free)
                            table.Cell().ColumnSpan(3).Element(Cell).AlignRight().PaddingRight(10)
                                .Text(t(lang, "Shipping")).SemiBold();
                            table.Cell().Element(Cell).Text(order.ShippingCost > 0
                                ? order.ShippingCost.ToString("C", euro)
                                : t(lang, "Free")).SemiBold();

                            // Grand Total
                            table.Cell().ColumnSpan(3).Element(Cell).AlignRight().PaddingRight(10)
                                .Text(t(lang, "GrandTotal")).SemiBold();
                            table.Cell().Element(Cell).Text(order.TotalAfterDiscount.ToString("C", euro)).SemiBold();

                            static IContainer Cell(IContainer container)
                            {
                                return container.PaddingVertical(5)
                                    .BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        });

                        // PAYMENT INFO (from config argument)
                        col.Item().PaddingTop(20).Column(c =>
                        {
                            c.Item().Text(t(lang, "PaymentInformation")).SemiBold();
                            c.Item().Text(t(lang, "PleaseTransfer"));

                            c.Item().Text($"{t(lang, "AccountHolder")}: {paymentInfo.AccountHolder}");
                            c.Item().Text($"{t(lang, "IBAN")}: {paymentInfo.IBAN}");
                            c.Item().Text($"{t(lang, "BIC")}: {paymentInfo.BIC}");
                            c.Item().Text($"{t(lang, "Bank")}: {paymentInfo.Bank}");


                            c.Item().PaddingTop(10).Text(t(lang, "ImportantNotice")).SemiBold();
                            c.Item().Text(t(lang, "OrderProcessedAfterPayment"));
                            c.Item().Text(t(lang, "OrderCancelledIfNoPayment"));
                        });

                        // Notes
                        col.Item().PaddingTop(20).Text(t(lang, "OrderNote"))
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                    });

                    // ========== FOOTER ==========
                    page.Footer().AlignCenter().Text(t(lang, "ThankYou"))
                        .FontSize(11).Italic().FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf();
        }
    }
}
