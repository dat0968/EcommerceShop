namespace APIClothesEcommerceShop.DTO
{
    public class ValidationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }

        public ValidationResult(bool isSuccess, string errorMessage = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
    }
}
