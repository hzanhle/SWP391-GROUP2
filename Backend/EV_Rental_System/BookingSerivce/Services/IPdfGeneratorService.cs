using BookingSerivce.DTOs;

namespace BookingSerivce.Services
{
    public interface IPdfGeneratorService
    {
        /// <summary>
        /// Generates a rental contract PDF from contract data.
        /// </summary>
        /// <param name="contractData">All contract information needed for PDF generation</param>
        /// <returns>PDF file as byte array</returns>
        Task<byte[]> GenerateContractPdfAsync(ContractData contractData);

        /// <summary>
        /// Validates that all required fields are present in contract data.
        /// </summary>
        bool ValidateContractData(ContractData contractData);
    }
}
