
namespace TinderClone.Models.Response
{
    public abstract class AbsResponse
    {
        public abstract string Message { get; set; }

        public abstract bool IsSuccess { get; set; }
    }
}
