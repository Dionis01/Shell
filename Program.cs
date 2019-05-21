using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;



namespace StudentParser
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Hello World!");
            foreach (var b in Subscriber.Load("C:\\Distr\\Data.csv")) ;

        }

    }

    class Subscriber
    {
        public IList<string> Body;
        public string PhoneNumber;



        public static IEnumerable<Subscriber> Load(string path)
        {
            String CurrentPhone = ""; //здесь храним текущий обрабатываемый номер, сначала его еще нет
            int GlobalCounter = 0; // счетчик событий во всей загрузке
            Subscriber ret = null;
            String Phone = "";
            String Separator = ";"; //разделитель данных в формате CSV
            String Cost = "";



            System.Data.SqlClient.SqlConnection sqlConnection1 =
           new System.Data.SqlClient.SqlConnection("Data Source=b-sql-test;Initial Catalog=MobileBase;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");


            foreach (var line in File.ReadLines(path, Encoding.GetEncoding("windows-1251")).Select(l => l.Trim()))
            {
                Decimal Final_Cost = 0;

                if (ret != null && line.StartsWith("Конец"))

                {
                    Console.WriteLine("Обработано " + GlobalCounter.ToString() + " строк");

                    yield return ret;
                    ret = null;
                    continue;
                }

                // определим новый номер или продолжаем рабоать со старым
                if (line.StartsWith("7"))
                {
                    if (CurrentPhone == "")
                    {
                        ret = new Subscriber { Body = new List<string>(), PhoneNumber = CurrentPhone };
                    }



                    Phone = line.Substring(0, 11); // выделяем из строки номер

                    if (CurrentPhone != Phone)
                    {
                        // здесь можно инициализирвоать новый объект для очередного номера телефона
                        CurrentPhone = Phone;
                        Console.WriteLine("Обработка номера: " + CurrentPhone);
                        //    continue;
                    }
                    GlobalCounter++;
                    string[] splitResult = Regex.Split(line, Separator);



                    Cost = splitResult[1];

                    Final_Cost = Convert.ToDecimal(Cost.Trim());
                    Final_Cost = Final_Cost / Convert.ToDecimal(1.2);


                    try
                    {

                        System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
                        cmd.CommandType = System.Data.CommandType.Text;

                       cmd.CommandText = "UPDATE employee  SET Subsciber = @Subscriber  WHERE phone_number = @phone_number";

                        {
                            // Добавить параметры
                            cmd.Parameters.AddWithValue("@phone_number", CurrentPhone.Trim());// получили номер
                            cmd.Parameters.AddWithValue("@Subscriber", Final_Cost);



                        }
                        cmd.Connection = sqlConnection1;
                        sqlConnection1.Open();
                        cmd.ExecuteNonQuery();
                        sqlConnection1.Close();


                    }
                    catch
                    {
                        sqlConnection1.Close();
                        Console.WriteLine("Ошибка Записи строки: " + GlobalCounter.ToString());

                    }
                }


            }



        }
    }
}
