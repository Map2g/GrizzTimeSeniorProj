using GrizzTime.BusinessLogic;

namespace GrizzTime.ViewModels
{
    public class BusinessVM
    {

    }

    public class ViewModelBusinessCreate : BusinessVM
    {
        public Business bus = new Business();
    }

    public class ViewModelBusinessList : BusinessVM
    {
        public Business.BusinessList businesses;
        public void Load()
        {
            businesses = new Business.BusinessList();
            businesses.Load();
        }
    }
}