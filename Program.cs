using Ninject;
using Ninject.Modules;
using System;
using System.Reflection;

namespace ConsoleApplication1
{
	public interface IMailSender
	{
		void Send(string toAddress, string subject);
	}
	public interface ILogging
	{
		void Debug(string message);
	}
	public class Logger : ILogging
	{
		public void Debug(string message)
		{
			Console.WriteLine("LOGGING: " + message);
		}
	}
	public class MailSender : IMailSender
	{
		public virtual void Send(string toAddress, string subject)
		{
			Console.WriteLine(string.Format("Sending mail to [{0}] with subject [{1}]", toAddress, subject));
		}
	}
	public class MailSenderEx : MailSender
	{
		private readonly ILogging logging;

		public MailSenderEx(ILogging l)
		{
			logging = l;
		}

		public override void Send(string toAddress, string subject)
		{
			logging.Debug("Sending mail");
			base.Send(toAddress, subject);
		}
	}
	public class FormHandler
	{
		private readonly IMailSender mailSender;
		public FormHandler([Named("NoLog")]IMailSender mailSender)
		{
			this.mailSender = mailSender;
		}
		public void Handle(string toAddress)
		{
			mailSender.Send(toAddress, "Hello from FormHandler");
		}
	}

	public class WithLogging : Attribute { }

	[WithLogging]
	public class FormHandlerEx : FormHandler
	{
		public FormHandlerEx(IMailSender mailSender) : base(mailSender)
		{
		}
	}
	//---------------------------------------------------------

	public class Binder : NinjectModule
	{
		public override void Load()
		{
			Bind<IMailSender>().To<MailSender>();
			Bind<IMailSender>().To<MailSender>().Named("NoLog");
			Bind<IMailSender>().To<MailSenderEx>().WhenClassHas<WithLogging>();
			Bind<ILogging>().To<Logger>();
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			IKernel k = new StandardKernel();
			k.Load(Assembly.GetExecutingAssembly());
			var d = k.Get<FormHandlerEx>();
			d.Handle("me@here.com");

			Console.ReadLine();
		}
	}
}
