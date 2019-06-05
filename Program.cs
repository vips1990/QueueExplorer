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
        private static MessageQueue sourceQueue, destinationQueue, localQueue;
        private string machine, machineIp, queueName, queueGUID, queuePath, queuePathIp;

        public Program()
        {
            machine = ConfigurationManager.AppSettings["MSMQMachineName"];
            machineIp = ConfigurationManager.AppSettings["MSMQMachineName_IP"];
            queueName = ConfigurationManager.AppSettings["QueueName"];
            queueGUID = ConfigurationManager.AppSettings["QueueID"];
            queuePath = machine + "\\" + queueName;
            queuePathIp = machineIp + "\\" + queueName;
            sourceQueue = new MessageQueue(string.Format("FormatName:DIRECT=TCP:{0}", queuePathIp + ";poison"), false);
            destinationQueue = new MessageQueue(string.Format("FormatName:DIRECT=TCP:{0}", queuePathIp + ";retry", false));
            localQueue = new MessageQueue(string.Format("FormatName:DIRECT=TCP:10.1.51.7\\bomireceiver"), false);
        }
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
            // Loop through the messages.
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

            Console.WriteLine("Total number of messages in poison queue are-" + msgs.Length);
            Console.ReadKey();

        }

        static void moveMessages()
        {

            try { 

                System.Console.WriteLine("      sourceQueue: " + sourceQueue.GetAllMessages().Count().ToString());
                System.Console.WriteLine("      destinationQueue: " + destinationQueue.GetAllMessages().Count().ToString());
                System.Console.WriteLine("      localQueue: " + localQueue.GetAllMessages().Count().ToString());


                System.Console.WriteLine(string.Format("Do you want to move {0} msg from  source queue to destination queue?Yes/No \n", sourceQueue.GetAllMessages().Count().ToString()));

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
