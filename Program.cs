using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.Net.Mail;
using System.Net;
using System.Configuration;

namespace QueueExplorer
{
    [Serializable]
    class Program
    {
        private static string machine = ConfigurationManager.AppSettings["MSMQMachineName"];
        private static string machineIp = ConfigurationManager.AppSettings["MSMQMachineName_IP"];
        private static string queueName = ConfigurationManager.AppSettings["QueueName"];
        private static string queueGUID = ConfigurationManager.AppSettings["QueueID"];
        private static string queuePath = machine + "\\" + queueName;
        private static string queuePathIp = machineIp + "\\" + queueName;
        private static MessageQueue sourceQueue = new MessageQueue(string.Format("FormatName:DIRECT=TCP:{0}", queuePathIp + ";poison"), false);
        private static MessageQueue destinationQueue = new MessageQueue(string.Format("FormatName:DIRECT=TCP:{0}", queuePathIp + ";retry"), false);
        private static MessageQueue localQueue = new MessageQueue(string.Format("FormatName:DIRECT=TCP:10.1.51.7\\bomireceiver"), false);

        static void Main(string[] args)
        {
            // count number of messages in the queue and display message body
            countAndReadMessages();

            // move messages between queues or sub queues
            moveMessages();
                                 
        }

        static void countAndReadMessages()
        {
            
            sourceQueue.MessageReadPropertyFilter.SetAll();
            destinationQueue.MessageReadPropertyFilter.SetAll();
            Message[] msgs = sourceQueue.GetAllMessages();

            if (msgs.Length < 1)
            {
                Console.WriteLine("Message Queue is Empty");
            }

            else { 
            
                // Read all messages from source queue
                foreach (Message msg in msgs)
                {
                    // Display the label of each message.
                    Console.WriteLine(msg.Label);

                    msg.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
                    // String message1 = msg.Body.ToString();
                    Console.WriteLine(msg.GetType());
                    Console.WriteLine("Arrived time: " + msg.ArrivedTime);
                    Console.WriteLine("Message label is: " + msg.Label);

                    byte[] data = new byte[2048];
                    msg.BodyStream.Read(data, 0, 2048);
                    string strMessage = ASCIIEncoding.ASCII.GetString(data);
                    Console.WriteLine(strMessage);
                    
                }

            }


            Console.WriteLine("Total number of messages in source queue are-" + msgs.Length);
            
        }

        static void moveMessages()
        {

            try { 

                
                System.Console.WriteLine("      sourceQueue: " + sourceQueue.GetAllMessages().Count().ToString());
                System.Console.WriteLine("      destinationQueue: " + destinationQueue.GetAllMessages().Count().ToString());
                System.Console.WriteLine("      localQueue: " + localQueue.GetAllMessages().Count().ToString());


                System.Console.WriteLine(string.Format("Do you want to move {0} msg from  source queue to destination queue?Yes/No \n", sourceQueue.GetAllMessages().Count().ToString()));

                // move all messages from source queue to destination queue
                if (System.Console.ReadLine().ToLower() == "yes")
                {
                    //var scope = new TransactionScope(TransactionScopeOption.Required);

                    foreach (var message in sourceQueue.GetAllMessages())
                    {
                        destinationQueue.Send(message, MessageQueueTransactionType.Single);

                    }

                    //scope.Complete();

                    Console.WriteLine("Messages moved..");
                    Console.WriteLine("Do you want to purgue the source queue? Yes/No \n");

                    // purge the messages in source queue
                    if (System.Console.ReadLine().ToLower() == "yes")
                    {

                        sourceQueue.Purge();
                        System.Console.WriteLine("SourceQueue: " + sourceQueue.GetAllMessages().Count().ToString());

                    }

                    
                    System.Console.WriteLine("");
                    System.Console.WriteLine("      Message after poison purge: ");
                    System.Console.WriteLine("      SourceQueue: " + sourceQueue.GetAllMessages().Count().ToString());
                    System.Console.WriteLine("      destinationQueue: " + destinationQueue.GetAllMessages().Count().ToString());
                    System.Console.WriteLine("      localQueue: " + localQueue.GetAllMessages().Count().ToString());

                }

            }

            catch(Exception ex)
            {
                throw ex;
            }
        }
    }

}
