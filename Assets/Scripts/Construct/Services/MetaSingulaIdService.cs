namespace Construct.Services
{
    public static class MetaSingulaIdService
    {
        private static int _currentId;

        public static int GetId() => --_currentId;
    }
}
