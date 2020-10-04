namespace SendPics
{
    class StateUser
    {
        public User User { get; set; }
        public int State { get; private set; }
        public int CurrentPage { get; set; }
        public string PhotoPath { get; set; }
        public string Category { get; set; }

        public StateUser(User user)
        {
            this.User = user;
            State = 0;
            PhotoPath = "";
            Category = "";
        }
        public void Kill()
        {
            if (State > 1)
            {
                System.IO.File.Delete(PhotoPath);
                PhotoPath = "";
            }
            Category = "";
            State = 0;
        }
        public void BeginProcess()
        {
            State = 1;
        }
        public void SetPath(string PhotoPath)
        {
            this.PhotoPath = PhotoPath;
            State = 2;
        }
        public void SetCategory(string Category)
        {
            this.Category = Category;
            State = 3;
        }
        public void Success()
        {
            if (State == 3)
            {
                State = 0;
                PhotoPath = "";
                Category = "";
            }
        }
    }
}
