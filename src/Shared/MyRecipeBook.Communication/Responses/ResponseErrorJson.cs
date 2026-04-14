namespace MyRecipeBook.Communication.Responses;
public class ResponseErrorJson
{
    public IList<string> Errors { get; set; }
    public bool TokenIsExpired { get; set; }

    public ResponseErrorJson(IList<string> errors, bool tokenIsExpired = false)
    {
        Errors = errors;
        TokenIsExpired = tokenIsExpired;
    }

    public ResponseErrorJson(string error, bool tokenIsExpired = false)
    {
        Errors = new List<string>
        {
            error
        };
        TokenIsExpired = tokenIsExpired;
    }
}
