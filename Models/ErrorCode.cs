namespace TestProd.Models
{
    public enum ErrorCode
    {
        None,
        ValidationError,
        InvalidUrlBase64,
        InvalidPageBase64,
        InvalidEncryptedTextBase64,
        InvalidKeyBase64,
        DecryptionError,
        DatabaseError,
        UnknownError
    }
}
