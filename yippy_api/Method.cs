
namespace Yippy
{
    public class Method
    {
        private string _name = string.Empty;
        private int _startPoint = 0;
        private int _endPoint = 1;

        public Method(string name, int startPoint, int endPoint)
        {
            _name = name;
            _startPoint = startPoint;
            _endPoint = endPoint;
        }

        public int StartPoint
        {
            get
            {
                return _startPoint;
            }
        }
        public int EndPoint
        {
            get
            {
                return _endPoint;
            }
        }
        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}
