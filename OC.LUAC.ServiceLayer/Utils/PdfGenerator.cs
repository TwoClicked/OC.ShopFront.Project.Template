using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OC.LUAC.ObjectLayer.Orders;

namespace OC.LUAC.ServiceLayer.Utils
{
    public static class PdfGenerator
    {
        public static byte[] GenerateOrderPdf(Order order)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // ========== HEADER ==========
                    var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "LUAC_DESIGN.png");
                    page.Header().Row(row =>
                    {
                        // Left side: Logo
                        row.ConstantItem(200) // controls logo box width
                            .Height(120)
                            .Image(logoPath, ImageScaling.FitArea);

                        // Right side: Order details
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().AlignRight().Text("Order Confirmation")
                                .FontSize(16).SemiBold().FontColor(Colors.Black);

                            col.Item().AlignRight()
                                .Text($"Order Number: {order.OrderNumber}")
                                .FontSize(11).FontColor(Colors.Grey.Darken2);

                            col.Item().AlignRight()
                                .Text($"Date: {order.CreatedAt:yyyy-MM-dd}")
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
                                c.Item().Text("Customer").SemiBold();
                                c.Item().Text($"{order.Customer?.FirstName} {order.Customer?.LastName}");
                                c.Item().Text(order.Customer?.Email ?? "");
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Shipping Address").SemiBold();
                                c.Item().Text($"{order.ShippingStreet} {order.ShippingNumber}");
                                c.Item().Text($"{order.ShippingPostalCode} {order.ShippingCity}");
                                c.Item().Text(order.ShippingCountry);
                            });
                        });

                        col.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // ORDER ITEMS
                        col.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(5); // product
                                columns.RelativeColumn(1); // qty
                                columns.RelativeColumn(2); // price
                                columns.RelativeColumn(2); // total
                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Product").SemiBold();
                                header.Cell().Element(CellStyle).Text("Qty").SemiBold();
                                header.Cell().Element(CellStyle).Text("Price").SemiBold();
                                header.Cell().Element(CellStyle).Text("Line Total").SemiBold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            decimal grandTotal = 0;

                            foreach (var item in order.Items)
                            {
                                var lineTotal = item.UnitPrice * item.Quantity;
                                grandTotal += lineTotal;

                                table.Cell().Element(Cell).Text($"{item.ProductName} ({item.Size})");
                                table.Cell().Element(Cell).Text(item.Quantity.ToString());
                                table.Cell().Element(Cell).Text($"{item.UnitPrice:C}");
                                table.Cell().Element(Cell).Text($"{lineTotal:C}");
                            }

                            // Grand total row
                            table.Cell().ColumnSpan(3).Element(Cell).AlignRight().PaddingRight(10).Text("Grand Total ").SemiBold();
                            table.Cell().Element(Cell).Text($"{grandTotal:C}").SemiBold();

                            static IContainer Cell(IContainer container)
                            {
                                return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        });

                        // Notes / info
                        col.Item().PaddingTop(20).Text("This is your order confirmation. Please keep it for your records.")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                    });

                    // ========== FOOTER ==========
                    page.Footer().AlignCenter().Text("Thank you for shopping with LUAC!")
                        .FontSize(11).Italic().FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf();
        }
    }
}
