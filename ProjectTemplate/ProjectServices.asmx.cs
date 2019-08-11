using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;


namespace ProjectTemplate
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]

    public class ProjectServices : System.Web.Services.WebService
    {
        ////////////////////////////////////////////////////////////////////////
        ///replace the values of these variables with your database credentials
        ////////////////////////////////////////////////////////////////////////
        private string dbID = "bscrum";
        private string dbPass = "!!Cis440";
        private string dbName = "bscrum";
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        ///call this method anywhere that you need the connection string!
        ////////////////////////////////////////////////////////////////////////
        private string getConString()
        {
            return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName + "; UID=" + dbID + "; PASSWORD=" + dbPass;
        }
        ////////////////////////////////////////////////////////////////////////



        /////////////////////////////////////////////////////////////////////////
        //don't forget to include this decoration above each method that you want
        //to be exposed as a web service!
        [WebMethod(EnableSession = true)]
        /////////////////////////////////////////////////////////////////////////
        public string TestConnection()
        {
            try
            {
                string testQuery = "select * from Users";

                ////////////////////////////////////////////////////////////////////////
                ///here's an example of using the getConString method!
                ////////////////////////////////////////////////////////////////////////
                MySqlConnection con = new MySqlConnection(getConString());
                ////////////////////////////////////////////////////////////////////////

                MySqlCommand cmd = new MySqlCommand(testQuery, con);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);
                return "Success!";
            }
            catch (Exception e)
            {
                return "Something went wrong, please check your credentials and db name and try again.  Error: " + e.Message;
            }
        }

        [WebMethod(EnableSession = true)] //NOTICE: gotta enable session on each individual method
        public bool LogOn(string uid, string pass)
        {
            //we return this flag to tell them if they logged in or not
            bool success = false;

            //our connection string comes from our web.config file like we talked about earlier
            string sqlConnectString = getConString();
            //here's our query.  A basic select with nothing fancy.  Note the parameters that begin with @
            //NOTICE: we added admin to what we pull, so that we can store it along with the id in the session
            string sqlSelect = "SELECT UserID FROM Users WHERE UserID=@UserID and Password=@Password";

            //set up our connection object to be ready to use our connection string
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            //set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //tell our command to replace the @parameters with real values
            //we decode them because they came to us via the web so they were encoded
            //for transmission (funky characters escaped, mostly)
            sqlCommand.Parameters.AddWithValue("@UserId", HttpUtility.UrlDecode(uid));
            sqlCommand.Parameters.AddWithValue("@Password", HttpUtility.UrlDecode(pass));

            //a data adapter acts like a bridge between our command object and 
            //the data we are trying to get back and put in a table object
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //here's the table we want to fill with the results from our query
            DataTable sqlDt = new DataTable();
            //here we go filling it!
            sqlDa.Fill(sqlDt);
            //check to see if any rows were returned.  If they were, it means it's 
            //a legit account
            if (sqlDt.Rows.Count > 0)
            {
                //if we found an account, store the id and admin status in the session
                //so we can check those values later on other method calls to see if they 
                //are 1) logged in at all, and 2) and admin or not
                Session["UserID"] = sqlDt.Rows[0]["UserID"];
                success = true;
            }
            //return the result!
            return success;
        }

        [WebMethod(EnableSession = true)]
        public string AddPost(string text, string category) //fixed, no errors when connecting to DB
        {
            Models.WhistleObjects PostInfo = new Models.WhistleObjects();
            PostInfo.Category = category;
            PostInfo.PostText = text;

            //flag as an array

            string sqlConnectString = getConString();
            string sqlStatement = "INSERT INTO Posts (Category, Text) VALUES(@category, @text)";

            //set up our connection object to be ready to use our connection string
            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            //set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlStatement, sqlConnection);


            sqlCommand.Parameters.AddWithValue("@category", HttpUtility.UrlDecode(category));
            sqlCommand.Parameters.AddWithValue("@text", HttpUtility.UrlDecode(text));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);

            DataTable sqlDt = new DataTable();
            //here we go filling it!
            sqlDa.Fill(sqlDt);

            return "Post added";
        }

        [WebMethod(EnableSession = true)]
        public Models.WhistleObjects[] GetPosts(string category)
        {//LOGIC: get all account requests and return them!
            Models.WhistleObjects PostInfo = new Models.WhistleObjects();
            PostInfo.Category = category;

            DataTable sqlDt = new DataTable();

            string sqlConnectString = getConString();
            //requests just have active set to 0
            string sqlSelect = "SELECT Text, Category, Flag, Votes, PostID FROM Posts WHERE Category=@category ORDER BY Votes DESC";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@category", HttpUtility.UrlDecode(category));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Models.WhistleObjects> listOfPosts = new List<Models.WhistleObjects>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                listOfPosts.Add(new Models.WhistleObjects
                {
                    Category = sqlDt.Rows[i]["Category"].ToString(),
                    PostText = sqlDt.Rows[i]["Text"].ToString(),
                    Flag = Int16.Parse(sqlDt.Rows[i]["Flag"].ToString()),
                    PostVotes = Int32.Parse(sqlDt.Rows[i]["Votes"].ToString()),
                    PostID = Int32.Parse(sqlDt.Rows[i]["PostID"].ToString())
                });
            }
            //convert the list of accounts to an array and return!
            return listOfPosts.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public Models.WhistleObjects[] TopPosts()
        {//LOGIC: get all account requests and return them!
            Models.WhistleObjects PostInfo = new Models.WhistleObjects();
            //PostInfo.Category = category;

            DataTable sqlDt = new DataTable();

            string sqlConnectString = getConString();
            //requests just have active set to 0
            string sqlSelect = "SELECT Text, Category, Flag, Votes, PostID FROM Posts ORDER BY Votes DESC limit 5";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //sqlCommand.Parameters.AddWithValue("@category", HttpUtility.UrlDecode(category));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Models.WhistleObjects> listOfPosts = new List<Models.WhistleObjects>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                listOfPosts.Add(new Models.WhistleObjects
                {
                    Category = sqlDt.Rows[i]["Category"].ToString(),
                    PostText = sqlDt.Rows[i]["Text"].ToString(),
                    Flag = Int16.Parse(sqlDt.Rows[i]["Flag"].ToString()),
                    PostVotes = Int32.Parse(sqlDt.Rows[i]["Votes"].ToString()),
                    PostID = Int32.Parse(sqlDt.Rows[i]["PostID"].ToString())
                });
            }
            //convert the list of accounts to an array and return!
            return listOfPosts.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public string UpvotePost(int id)
        {//LOGIC: get postid and upvote it
            Models.WhistleObjects PostInfo = new Models.WhistleObjects();
            PostInfo.PostID = id;

            DataTable sqlDt = new DataTable();

            string sqlConnectString = getConString();
            //requests just have active set to 0
            string sqlUpdate = "UPDATE Posts SET Votes = Votes + 1 WHERE PostID=@id";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlUpdate, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@id", HttpUtility.UrlDecode(id.ToString()));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            return "Post upvoted!";
        }

        [WebMethod(EnableSession = true)]
        public Models.WhistleObjects[] GetPostsbyText(string postText)
        {//LOGIC: get all account requests and return them!
            Models.WhistleObjects PostInfo = new Models.WhistleObjects();
            PostInfo.PostText = postText;

            DataTable sqlDt = new DataTable();

            string sqlConnectString = getConString();
            //requests just have active set to 0
            string sqlSelect = "SELECT Text, Category, Flag, Votes, PostID FROM Posts WHERE Text LIKE '%" + @postText + "%' ORDER BY Votes DESC";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@postText", HttpUtility.UrlDecode(postText));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Models.WhistleObjects> listOfPosts = new List<Models.WhistleObjects>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                listOfPosts.Add(new Models.WhistleObjects
                {
                    Category = sqlDt.Rows[i]["Category"].ToString(),
                    PostText = sqlDt.Rows[i]["Text"].ToString(),
                    Flag = Int16.Parse(sqlDt.Rows[i]["Flag"].ToString()),
                    PostVotes = Int32.Parse(sqlDt.Rows[i]["Votes"].ToString()),
                    PostID = Int32.Parse(sqlDt.Rows[i]["PostID"].ToString())
                });
            }
            //convert the list of accounts to an array and return!
            return listOfPosts.ToArray();
        }


        [WebMethod(EnableSession = true)]
        public string FlagPost(int id)
        {//LOGIC: get postid and upvote it
            Models.WhistleObjects PostInfo = new Models.WhistleObjects();
            PostInfo.PostID = id;

            DataTable sqlDt = new DataTable();

            string sqlConnectString = getConString();
            //requests just have active set to 0
            string sqlUpdate = "UPDATE Posts SET Flag = 1 WHERE PostID=@id";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlUpdate, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@id", HttpUtility.UrlDecode(id.ToString()));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            return "Post Flagged";
        }

        [WebMethod(EnableSession = true)]
        public Models.WhistleObjects[] FlaggedPosts()
        {//LOGIC: get all account requests and return them!
            Models.WhistleObjects PostInfo = new Models.WhistleObjects();


            DataTable sqlDt = new DataTable();

            string sqlConnectString = getConString();
            //requests just have active set to 0
            string sqlSelect = "SELECT Text, Category, Flag, Votes, PostID FROM Posts WHERE Flag=1";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Models.WhistleObjects> listOfPosts = new List<Models.WhistleObjects>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                listOfPosts.Add(new Models.WhistleObjects
                {
                    Category = sqlDt.Rows[i]["Category"].ToString(),
                    PostText = sqlDt.Rows[i]["Text"].ToString(),
                    Flag = Int16.Parse(sqlDt.Rows[i]["Flag"].ToString()),
                    PostVotes = Int32.Parse(sqlDt.Rows[i]["Votes"].ToString()),
                    PostID = Int32.Parse(sqlDt.Rows[i]["PostID"].ToString())
                });
            }
            //convert the list of accounts to an array and return!
            return listOfPosts.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public string DeletePost(int id)
        {//LOGIC: get postid and upvote it
            Models.WhistleObjects PostInfo = new Models.WhistleObjects();
            PostInfo.PostID = id;

            DataTable sqlDt = new DataTable();

            string sqlConnectString = getConString();
            //requests just have active set to 0
            string sqlUpdate = "Delete From Posts WHERE PostID=@id";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlUpdate, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@id", HttpUtility.UrlDecode(id.ToString()));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            return "Post Deleted";
        }


    }
}
