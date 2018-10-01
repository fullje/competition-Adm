using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EasyCrypto;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using HtmlAgilityPack;

/* [Data] 2018 - 08 - 17
 * - Weryfikacja czy pola od czasu to INT
 * - Ogarniecie tego, ze po kliknieciu na puste pole z grida wywala error
 * - 
 * 
 * 
 * 
 * 
 * 
 */

namespace MaturaZGier
{

    public partial class MainWindow : Window
    {
        loadData ld = new loadData();
        connectDB conn = new connectDB();

        public MainWindow()
        {
            //Salt to password!!
            
            InitializeComponent();
            loadAll();

            

            //MessageBox.Show(getTime().ToString());
        }
        
        private void loadAll()
        {
            // Poprawic bo wyglada bedzie okropnie :v
            // Poprawne odpowiedzi do pytan
            selectCorrectAnswer.Items.Add("A");
            selectCorrectAnswer.Items.Add("B");
            selectCorrectAnswer.Items.Add("C");
            selectCorrectAnswer.Items.Add("D");

            //DO NOT ALLOW TO CHANGE THIS UNTIL USER LOGING IN CORRECTLY
            setState(false);

        }

        private int getTime() // To tylko dla klienta
        {
            var url = @"https://www.unixtimestamp.com/";
            var web = new HtmlWeb();
            var webData = web.Load(url);

            int timeInSec = 0;

            string trash = null;
            string unixSec = null;

            var htmlNode = webData.DocumentNode.SelectSingleNode("//body/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/p/h3");
            if (null != htmlNode)
            {
                var htmlRemove = webData.DocumentNode.SelectSingleNode("//body/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/p/h3/small");

                trash = htmlRemove.OuterHtml;
                unixSec = htmlNode.InnerHtml;
                //MessageBox.Show(htmlNode.InnerHtml);
            }

            StringBuilder sb = new StringBuilder(unixSec);

            timeInSec = Convert.ToInt32(sb.Replace(trash, "").ToString());
           
            DateTime dt = new DateTime(1970, 1, 1, 2, 0, 0).AddSeconds(timeInSec);
            //MessageBox.Show(dt.ToString("yyyy:dd:MM - H:mm:ss"));

            return timeInSec;
        }

        private void setState(bool state)
        {
            questionContent.IsEnabled = state;
            answerA.IsEnabled = state;
            answerB.IsEnabled = state;
            answerC.IsEnabled = state;
            answerD.IsEnabled = state;
            reloadBtn.IsEnabled = state;
            selectDate.IsEnabled = state;
            hTxt.IsEnabled = state;
            mTxt.IsEnabled = state;
            setTime.IsEnabled = state;

            questionNumber.IsEnabled = state;
            selectCorrectAnswer.IsEnabled = state;
            sendBtn.IsEnabled = state;
        }

        private void encryptBtn_Click(object sender, RoutedEventArgs e) // Needs to add PathFile
        {
            try
            {
                ld.encodeConnection(passwordTxt.Password, addresBox.Text, passwordBox.Password);
            }catch(Exception)
            {
                MessageBox.Show("Popraw hasło");
            }
        }


        private void decryptBtn_Click(object sender, RoutedEventArgs e) // Needs to add PathFile and ONLY IN CLIENT APP
        {
            try
            {
                ld.decodeConnection(passwordTxt.Password);
            }catch (Exception)
            {
                MessageBox.Show("Popraw hasło");
            }
        }

        private void createDBBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                conn.createDB();
                createDBBtn.IsEnabled = false;
            }
            catch(Exception xe)
            {
                MessageBox.Show("Error: " + xe.Message);
                createDBBtn.IsEnabled = true;
            }
            
        }

        private void applyBtn_Click(object sender, RoutedEventArgs e)
        { 
            try
            {
                conn.connection(addressDBTxt.Text, passwordDBTxt.Text);
                MessageBox.Show("Zalogowano pomyslnie do bazy danych!");
                setState(true);
                //DataGrid
                viewDataBase.ItemsSource = conn.selectDB().DefaultView;
                scoreGrid.ItemsSource = conn.scoreDB().DefaultView;
                timeGrid.ItemsSource = conn.selectTimeDB().DefaultView;
            }
            catch (Exception xe)
            {
                MessageBox.Show("Error: " + xe.Message);
            }

            try
            {
                conn.checkDB();
                createDBBtn.IsEnabled = false;
            }
            catch(Exception xe)
            {
                MessageBox.Show("Error: " + xe.Message);
                createDBBtn.IsEnabled = true;
            }

            try
            {
                DataTable dt = conn.selectTimeDB();
                //MessageBox.Show(dt.Rows.Count.ToString());

                if(dt.Rows.Count > 0)
                {
                    setTime.IsEnabled = false;
                    updateTime.IsEnabled = true;
                }
            }
            catch (Exception xe)
            {
                MessageBox.Show("Error: " + xe.Message);
            }
        }

        private void sendBtn_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string number = questionNumber.Text;
                string content = questionContent.Text;
                string qA = answerA.Text;
                string qB = answerB.Text;
                string qC = answerC.Text;
                string qD = answerD.Text;
                string correctA = selectCorrectAnswer.SelectedItem.ToString();

                //MessageBox.Show(number.ToString()+" "+content+ " " + qA+ " " + qB+" "+qC+" "+qD+" "+correctA);
                conn.insertDB(Convert.ToInt32(number), content, qA, qB, qC, qD, correctA);

                clearViewOfBoxes();
            }
            catch (Exception)
            {
                //MessageBox.Show("Error: " + xe.Message);
                MessageBox.Show("Sprawdz czy wszystkie pola zostały poprawnie uzupełnione");
            }
            
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string number = questionNumber.Text;
                string content = questionContent.Text;
                string qA = answerA.Text;
                string qB = answerB.Text;
                string qC = answerC.Text;
                string qD = answerD.Text;
                string correctA = selectCorrectAnswer.SelectedItem.ToString();

                conn.updateDB(Convert.ToInt32(number), content, qA, qB, qC, qD, correctA);

                clearViewOfBoxes();

                updateBtn.IsEnabled = false;
                deleteBtn.IsEnabled = false;
                sendBtn.IsEnabled = true;
            }
            catch(Exception)
            {
                MessageBox.Show("Sprawdz czy wszystkie pola zostały poprawnie uzupełnione");
            }

            
        }


        private void viewDataBase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateBtn.IsEnabled = true;
            sendBtn.IsEnabled = false;
            deleteBtn.IsEnabled = true;
            DataRowView drv = viewDataBase.SelectedItem as DataRowView;

            //MessageBox.Show(drv.Row["numerPytania"].ToString());
            

            if(drv != null)
            {
                questionNumber.Text = drv.Row["NumerPytania"].ToString();
                questionContent.Text = drv.Row["TrescPytania"].ToString();
                answerA.Text = drv.Row["OdpowiedzA"].ToString();
                answerB.Text = drv.Row["OdpowiedzB"].ToString();
                answerC.Text = drv.Row["OdpowiedzC"].ToString();
                answerD.Text = drv.Row["OdpowiedzD"].ToString();
                selectCorrectAnswer.Text = drv.Row["PoprawnaOdpowiedz"].ToString();
            }
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            DataRowView drv = viewDataBase.SelectedItem as DataRowView;
            if (null != drv)
            {
                conn.deleteDB(Convert.ToInt32(drv.Row["NumerPytania"].ToString()));
                clearViewOfBoxes();
                deleteBtn.IsEnabled = false;
            }
            
        }

        private void clearViewOfBoxes()
        {
            viewDataBase.ItemsSource = conn.selectDB().DefaultView;

            questionNumber.Clear();
            questionContent.Clear();
            answerA.Clear();
            answerB.Clear();
            answerC.Clear();
            answerD.Clear();
        }

        private void reloadBtn_Click(object sender, RoutedEventArgs e)
        {
            scoreGrid.ItemsSource = conn.scoreDB().DefaultView;
        }

        private void setTime_Click(object sender, RoutedEventArgs e)
        {
            if (selectDate.SelectedDate == null || hTxt.Text == "" || mTxt.Text == "" || (Convert.ToInt32(hTxt.Text.ToString()) > 24) || (Convert.ToInt32(mTxt.Text.ToString()) > 59))
            {
                MessageBox.Show("Musisz uzupełnić pola dotyczące czasu poprawnie!");
            }
            else
            {
                try
                {
                    string startTime = (selectDate.Text + ", " + hTxt.Text + ":" + mTxt.Text);
                    conn.insertDB(startTime);
                    timeGrid.ItemsSource = conn.selectTimeDB().DefaultView;
                    selectDate.SelectedDate = null;
                    hTxt.Text = "";
                    mTxt.Text = "";
                    setTime.IsEnabled = false;
                    updateTime.IsEnabled = true;
                }
                catch (Exception xe)
                {
                    MessageBox.Show("Error: " + xe.Message);
                }
            }
        }

     

        private void updateTime_Click(object sender, RoutedEventArgs e)
        {
            string date = (selectDate.Text + ", " + hTxt.Text + ":" + mTxt.Text);

            if (selectDate.SelectedDate == null || hTxt.Text == "" || mTxt.Text == "" || (Convert.ToInt32(hTxt.Text.ToString()) > 24) || (Convert.ToInt32(mTxt.Text.ToString()) > 59))
            {
                MessageBox.Show("Musisz uzupełnić pola dotyczące czasu poprawnie!");
            }
            else
            {
                try
                {
                    conn.updateDB(date);
                    timeGrid.ItemsSource = conn.selectTimeDB().DefaultView;
                    selectDate.SelectedDate = null;
                    hTxt.Text = "";
                    mTxt.Text = "";
                }
                catch (Exception xe)
                {
                    MessageBox.Show("Error: " + xe.Message);
                }
            }
        }
        
    }

    class connectDB
    {
        static string setConnection;
        MySqlConnection questionDB;

        public void connection(string server, string password)
        {
            setConnection = "SERVER="+ server +";DATABASE=matura;User ID=root;Password="+ password +";SSLmode=none";
            questionDB = new MySqlConnection(setConnection);
            questionDB.Open();

            questionDB.Close();

        }

        public void checkDB()
        {
            string query = "DESCRIBE matura;";
            MySqlCommand insertCommand = new MySqlCommand(query, questionDB);
            MySqlDataReader reader;
            questionDB.Open();

            reader = insertCommand.ExecuteReader();

            reader.Close();
            questionDB.Close();
        }

        public void createDB()
        {
            //Create DB
            questionDB.Close();
            string query = "CREATE TABLE matura (id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, NumerPytania INT, TrescPytania VARCHAR(200), OdpowiedzA VARCHAR(200), OdpowiedzB VARCHAR(200), OdpowiedzC VARCHAR(200), OdpowiedzD VARCHAR(200), PoprawnaOdpowiedz VARCHAR(200));";
            string query1 = "CREATE TABLE konkurs (id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, Nick VARCHAR(200), email VARCHAR(200), CorrectAnswers INT, Time varchar(200));";
            string query2 = "CREATE TABLE eventStart (id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, date VARCHAR(200));";
            //TABELA KONKURS 
            //ID, Nick, ilosc poprawnych odpowiedzi, CZAS nadesłania gotowego formularza
            //
        
            MySqlCommand commandInsert = new MySqlCommand(query, questionDB);
            MySqlCommand commandInsert1 = new MySqlCommand(query1, questionDB);
            MySqlCommand commandInsert2 = new MySqlCommand(query2, questionDB);
            MySqlDataReader reader;
            questionDB.Open();

            reader = commandInsert.ExecuteReader();
            reader.Close();

            reader = commandInsert1.ExecuteReader();
            reader.Close();

            reader = commandInsert2.ExecuteReader();
            reader.Close();

            questionDB.Close();

        }

        public void insertDB(string time) //Przeciazenie
        {
            string query = "INSERT INTO eventStart (date) VALUES ('" + time + "');";
            
            MySqlCommand commandInsert = new MySqlCommand(query, questionDB);
            MySqlDataReader reader;
            questionDB.Open();

            reader = commandInsert.ExecuteReader();

            reader.Close();
            questionDB.Close();
        }

        public void insertDB(int questionNumber, string questionContent, string qA, string qB, string qC, string qD, string correctQ)
        {
            string query = "INSERT INTO matura (NumerPytania, TrescPytania, OdpowiedzA, OdpowiedzB, OdpowiedzC, OdpowiedzD, PoprawnaOdpowiedz) VALUES ("+ questionNumber + ",'" + questionContent + "'" + ",'" + qA + "'" + ",'" + qB + "'" + ",'" + qC + "'" + ",'" + qD + "'" + ",'" + correctQ + "');";
            //string query = "INSERT INTO matura(NumerPytania, TrescPytania, OdpowiedzA, OdpowiedzB, OdpowiedzC, OdpowiedzD, PoprawnaOdpowiedz) VALUES (11, 'pytanie', 'aa' , 'bb', 'cc', 'dd', 'A');";
            //MessageBox.Show(query);
            MySqlCommand commandInsert = new MySqlCommand(query, questionDB);
            MySqlDataReader reader;
            questionDB.Open();

            reader = commandInsert.ExecuteReader();

            reader.Close();
            questionDB.Close();
        }

        public DataTable selectDB()
        {
            string query = "SELECT * FROM matura";
            MySqlCommand commandSelect = new MySqlCommand(query, questionDB);
            MySqlDataAdapter adp = new MySqlDataAdapter(commandSelect);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            questionDB.Open();

            questionDB.Close();

            return dt;
        }

        public DataTable selectTimeDB()
        {
            //string query = "SELECT * FROM eventStart";
            string query = "select DATE_FORMAT(date, '%Y %m %d, %H:%i') from eventStart";
            MySqlCommand commandSelect = new MySqlCommand(query, questionDB);
            MySqlDataAdapter adp = new MySqlDataAdapter(commandSelect);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            questionDB.Open();

            questionDB.Close();

            return dt;
        }

        public DataTable scoreDB()
        {
            string query = "SELECT * FROM konkurs";
            MySqlCommand commandSelect = new MySqlCommand(query, questionDB);
            MySqlDataAdapter adp = new MySqlDataAdapter(commandSelect);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            questionDB.Open();

            questionDB.Close();

            return dt;
        }

        public void updateDB(string time)
        {
            string query = "UPDATE eventStart SET date='" + time + "' WHERE (id=1);";
            //string query = "INSERT INTO matura(NumerPytania, TrescPytania, OdpowiedzA, OdpowiedzB, OdpowiedzC, OdpowiedzD, PoprawnaOdpowiedz) VALUES (11, 'pytanie', 'aa' , 'bb', 'cc', 'dd', 'A');";
            //MessageBox.Show(query);
            MySqlCommand commandInsert = new MySqlCommand(query, questionDB);
            MySqlDataReader reader;
            questionDB.Open();

            reader = commandInsert.ExecuteReader();

            reader.Close();
            questionDB.Close();
        }

        public void updateDB(int questionNumber, string questionContent, string qA, string qB, string qC, string qD, string correctQ)
        {
            string query = "UPDATE matura SET " +
                "TrescPytania='" + questionContent + "'" + 
                ",OdpowiedzA='" + qA + "'" + 
                ",OdpowiedzB='" + qB + "'" + 
                ",OdpowiedzC='" + qC + "'" + 
                ",OdpowiedzD='" + qD + "'" + 
                ",PoprawnaOdpowiedz='" + correctQ + 
                "' WHERE " +
                "(NumerPytania=" + questionNumber + 
                ");";
            //string query = "INSERT INTO matura(NumerPytania, TrescPytania, OdpowiedzA, OdpowiedzB, OdpowiedzC, OdpowiedzD, PoprawnaOdpowiedz) VALUES (11, 'pytanie', 'aa' , 'bb', 'cc', 'dd', 'A');";
            //MessageBox.Show(query);
            MySqlCommand commandInsert = new MySqlCommand(query, questionDB);
            MySqlDataReader reader;
            questionDB.Open();

            reader = commandInsert.ExecuteReader();

            reader.Close();
            questionDB.Close();
        }

        public void deleteDB(int questionNumber)
        {
            string query = "DELETE FROM matura WHERE (NumerPytania=" + questionNumber + ");";
            //string query = "INSERT INTO matura(NumerPytania, TrescPytania, OdpowiedzA, OdpowiedzB, OdpowiedzC, OdpowiedzD, PoprawnaOdpowiedz) VALUES (11, 'pytanie', 'aa' , 'bb', 'cc', 'dd', 'A');";
            //MessageBox.Show(query);
            MySqlCommand commandInsert = new MySqlCommand(query, questionDB);
            MySqlDataReader reader;
            questionDB.Open();

            reader = commandInsert.ExecuteReader();

            reader.Close();
            questionDB.Close();
        }

    }

    class loadData
    {
        //string salt = "!@3esxC$Wqasdrt6yUJKO(*&";

        public void encodeConnection(string p, string aDB, string pDB)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            string password = p; // Key for encryption
            string serverDB = AesEncryption.EncryptWithPassword(aDB, password); // Server DB
            string passwdDB = AesEncryption.EncryptWithPassword(pDB, password); // Password to DB


            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("kycu"); // Server adrdess
                writer.WriteValue(serverDB);
                writer.WritePropertyName("losiu"); // Password
                writer.WriteValue(passwdDB);
                writer.WriteEndObject();

            }

            File.WriteAllText(@"C:\Projekty\TicketApp\MaturaZGier\keys.json", sb.ToString());
            MessageBox.Show("Para kluczy została wygenerowana");

        }

        public void decodeConnection(string password) // Args, patch to the file
        {
            string st = File.ReadAllText(@"C:\Projekty\TicketApp\MaturaZGier\keys.json");

            var jPerson = JsonConvert.DeserializeObject<dynamic>(st);

            string serverAddr = jPerson.kycu.ToString();
            serverAddr = AesEncryption.DecryptWithPassword(serverAddr, password);
            string serverPass = jPerson.losiu.ToString();
            serverPass = AesEncryption.DecryptWithPassword(serverPass, password);

            MessageBox.Show("Pomyślnie ustanowiono połączenie");
           
        }

        public string decrypt(string message, string password)
        {
            return AesEncryption.DecryptWithPassword(message, password);
        }
    }
}
