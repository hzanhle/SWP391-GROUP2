using BookingSerivce.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookingSerivce.Services
{
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private readonly ILogger<PdfGeneratorService> _logger;

        public PdfGeneratorService(ILogger<PdfGeneratorService> logger)
        {
            _logger = logger;

            // Set QuestPDF license (Community license for non-commercial use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateContractPdfAsync(ContractData contractData)
        {
            try
            {
                if (!ValidateContractData(contractData))
                {
                    throw new ArgumentException("Contract data is missing required fields");
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        page.Header().Element(ComposeHeader);
                        page.Content().Element(content => ComposeContent(content, contractData));
                        page.Footer().Element(ComposeFooter);
                    });
                });

                // Generate PDF asynchronously
                return await Task.Run(() => document.GeneratePdf());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate PDF for contract {ContractNumber}", contractData.ContractNumber);
                throw;
            }
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("EV RENTAL SYSTEM")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Medium);

                    column.Item().Text("Electric Vehicle Rental Contract")
                        .FontSize(14)
                        .FontColor(Colors.Grey.Darken2);
                });

                row.ConstantItem(120).AlignRight().Column(column =>
                {
                    column.Item().Text($"Contract #")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);

                    column.Item().Text("Contract Date:")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });
            });
        }

        private void ComposeContent(IContainer container, ContractData data)
        {
            container.Column(column =>
            {
                column.Spacing(15);

                // Contract Number and Date
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Contract Number: {data.ContractNumber}")
                        .Bold().FontSize(12);
                    row.RelativeItem().AlignRight().Text($"Date: {data.ContractDate:dd/MM/yyyy}")
                        .FontSize(11);
                });

                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Renter Information
                column.Item().Text("RENTER INFORMATION").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120);
                        columns.RelativeColumn();
                    });

                    table.Cell().Text("Full Name:");
                    table.Cell().Text(data.UserFullName);

                    table.Cell().Text("Email:");
                    table.Cell().Text(data.UserEmail);

                    table.Cell().Text("Phone:");
                    table.Cell().Text(data.UserPhone);

                    table.Cell().Text("ID Card:");
                    table.Cell().Text(data.UserIdCard);

                    table.Cell().Text("Address:");
                    table.Cell().Text(data.UserAddress);
                });

                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Vehicle Information
                column.Item().Text("VEHICLE INFORMATION").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120);
                        columns.RelativeColumn();
                    });

                    table.Cell().Text("Vehicle:");
                    table.Cell().Text($"{data.VehicleBrand} {data.VehicleModel}");

                    table.Cell().Text("Plate Number:");
                    table.Cell().Text(data.VehiclePlateNumber);

                    table.Cell().Text("Color:");
                    table.Cell().Text(data.VehicleColor);

                    table.Cell().Text("Hourly Rate:");
                    table.Cell().Text($"{data.HourlyRate:N0} VND/hour");
                });

                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Rental Period
                column.Item().Text("RENTAL PERIOD").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120);
                        columns.RelativeColumn();
                    });

                    table.Cell().Text("Start Date:");
                    table.Cell().Text(data.FromDate.ToString("dd/MM/yyyy HH:mm"));

                    table.Cell().Text("End Date:");
                    table.Cell().Text(data.ToDate.ToString("dd/MM/yyyy HH:mm"));

                    table.Cell().Text("Total Duration:");
                    table.Cell().Text($"{data.TotalDays} day(s)");
                });

                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Payment Information
                column.Item().Text("PAYMENT DETAILS").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120);
                        columns.RelativeColumn();
                    });

                    table.Cell().Text("Total Rental Cost:");
                    table.Cell().Text($"{data.TotalCost:N0} VND");

                    table.Cell().Text("Deposit Amount:");
                    table.Cell().Text($"{data.DepositAmount:N0} VND ({data.DepositPercentage:P0})");

                    table.Cell().Text("Transaction ID:");
                    table.Cell().Text(data.TransactionId);

                    table.Cell().Text("Payment Method:");
                    table.Cell().Text(data.PaymentMethod);

                    table.Cell().Text("Paid At:");
                    table.Cell().Text(data.PaidAt.ToString("dd/MM/yyyy HH:mm"));

                    table.Cell().Text("Paid Amount:").Bold();
                    table.Cell().Text($"{data.PaidAmount:N0} VND").Bold().FontColor(Colors.Green.Darken2);
                });

                column.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Grey.Medium);

                // Terms and Conditions
                column.Item().Text("TERMS AND CONDITIONS").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);
                column.Item().Text(text =>
                {
                    text.Span("1. ").Bold();
                    text.Span("The renter agrees to use the vehicle responsibly and return it in the same condition.\n");

                    text.Span("2. ").Bold();
                    text.Span("The renter is responsible for any damage or loss during the rental period.\n");

                    text.Span("3. ").Bold();
                    text.Span("The deposit will be refunded after vehicle inspection upon return.\n");

                    text.Span("4. ").Bold();
                    text.Span("Late returns will incur additional charges at the hourly rate.\n");

                    text.Span("5. ").Bold();
                    text.Span("The renter must have a valid driver's license for the vehicle category.\n");
                });

                // Signatures
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Renter Signature").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(40).LineHorizontal(1);
                        col.Item().Text($"Name: {data.UserFullName}").FontSize(9);
                        col.Item().Text($"Date: {data.ContractDate:dd/MM/yyyy}").FontSize(9);
                    });

                    row.ConstantItem(50);

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Company Representative").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(40).LineHorizontal(1);
                        col.Item().Text("EV Rental System").FontSize(9);
                        col.Item().Text($"Date: {data.ContractDate:dd/MM/yyyy}").FontSize(9);
                    });
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("This is an auto-generated contract. For questions, contact support@evrental.com | ").FontSize(9).FontColor(Colors.Grey.Darken1);
                text.Span("Page ").FontSize(9).FontColor(Colors.Grey.Darken1);
                text.CurrentPageNumber().FontSize(9);
                text.Span(" of ").FontSize(9).FontColor(Colors.Grey.Darken1);
                text.TotalPages().FontSize(9);
            });
        }

        public bool ValidateContractData(ContractData contractData)
        {
            if (contractData == null) return false;

            return !string.IsNullOrEmpty(contractData.ContractNumber) &&
                   !string.IsNullOrEmpty(contractData.UserFullName) &&
                   !string.IsNullOrEmpty(contractData.UserEmail) &&
                   !string.IsNullOrEmpty(contractData.VehicleBrand) &&
                   !string.IsNullOrEmpty(contractData.VehicleModel) &&
                   !string.IsNullOrEmpty(contractData.VehiclePlateNumber) &&
                   !string.IsNullOrEmpty(contractData.TransactionId) &&
                   contractData.TotalCost > 0 &&
                   contractData.DepositAmount > 0;
        }
    }
}
