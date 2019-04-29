using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.Net.Mail;
using System.Net;

namespace QueueExplorer
{
    [Serializable]
    class Program
    {
        static void Main(string[] args)
        {

            MessageQueue queue = new MessageQueue("BtMSMQ\\bomireceiver;poison");
            // MessageQueue queue = new MessageQueue("PUBLIC=8A2FF6B3-3305-4CF3-A0E3-6F3A5E8597EE;poison");
            MessageQueue destinationqueue = new MessageQueue("BtMSMQ\\bomireceiver;retry");
            bool NoMessage = true;
            // Populate an array with copies of all the messages in the queue.

            queue.MessageReadPropertyFilter.SetAll();
            destinationqueue.MessageReadPropertyFilter.SetAll();
            Message[] msgs = queue.GetAllMessages();

            // Loop through the messages.
            foreach (Message msg in msgs)
            {
                // Display the label of each message.
                Console.WriteLine(msg.Label);

                msg.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
                // String message1 = msg.Body.ToString();
                Console.WriteLine(msg.GetType());
                Console.WriteLine("Arrived time: "+ msg.ArrivedTime);
                Console.WriteLine("Message label is: "+msg.Label);

                NoMessage = false;
                byte[] data = new byte[2048];
                msg.BodyStream.Read(data, 0, 2048);
                string strMessage = ASCIIEncoding.ASCII.GetString(data);
                Console.WriteLine(strMessage);
                
                
                msg.Priority = System.Messaging.MessagePriority.Normal;
                msg.Recoverable = true;
                
                destinationqueue.DefaultPropertiesToSend.Recoverable = true;
                destinationqueue.DefaultPropertiesToSend.UseEncryption = true;
                
                // destinationqueue.DefaultPropertiesToSend.UseAuthentication = true;
                // Console.WriteLine(destinationqueue.Transactional);


                
                MessageQueueTransaction myTransaction = new MessageQueueTransaction();

                try
                {
                    // Begin a transaction.
                    myTransaction.Begin();
                    Console.WriteLine("Inside transaction ");
                    destinationqueue.Send(msg, myTransaction);

                    myTransaction.Commit();

                }

                catch (System.Exception e)
                {
                    myTransaction.Abort();
                    throw e;

                }

                finally
                {
                    // Dispose of the transaction object.
                    myTransaction.Dispose();
                        
                }
                

                break;
                        

            }

            if (NoMessage)
            {
                Console.WriteLine("Message Queue is Empty");
            }

            Console.WriteLine("Total number of messages in poison queue are-" + msgs.Length);
            Console.ReadKey();

            
        }



    }
}
