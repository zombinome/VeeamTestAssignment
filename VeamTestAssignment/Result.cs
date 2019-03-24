namespace VeeamTestAssignment
{
    internal class Result<T>
    {
        public readonly T Value;

        public readonly bool Success;

        public readonly string Error;

        public Result(T value)
        {
            this.Success = true;
            this.Value = value;
        }

        public Result(string error)
        {
            this.Success = false;
            this.Error = error;
        }
    }
}
