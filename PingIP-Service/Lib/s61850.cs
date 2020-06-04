using System;
using IEC61850.Server;
using IEC61850.Common;
using IEC61850.GOOSE.Subscriber;

namespace PingIP_Service
{
    class s61850
    {
		private IedModel iedModel;
		private IedServerConfig config;
		private IedServer iedServer;
		public s61850()
		{
			iedModel = ConfigFileParser.CreateModelFromConfigFile("model.cfg");
			if (iedModel == null)
			{
				Console.WriteLine("SYSERR: No Valid DataModel Found!");
				return;
			}
			

			config = new IedServerConfig();
			config.ReportBufferSize = 100000;

			iedServer = new IedServer(iedModel, config);


			iedServer.Start(10102);
			Console.WriteLine("SYSLOG: Iec61850 Server is Listening on port 10102.");

			GC.Collect();

			/*//Interface id: 0(\Device\NPF_{ 874C4A5F - 2D90 - 42E8 - AFD3 - E76B65365490})
			Console.WriteLine("Starting GOOSE subscriber...");

			GooseReceiver receiver = new GooseReceiver();

			receiver.SetInterfaceId(@"0");

			GooseSubscriber subscriber = new GooseSubscriber("TIESYS/LLN0$GO$gcb01");

			subscriber.SetAppId(1000);

			subscriber.SetListener(gooseListener, null);

			receiver.AddSubscriber(subscriber);

			receiver.Start();*/
		}

		public void ModifyFloatValue(string ObjRef, string value)
		{
			DataObject DataObj = (DataObject)iedModel.GetModelNodeByShortObjectReference(ObjRef);

			DataAttribute DataObj_F = (DataAttribute)DataObj.GetChild("mag.f");
			DataAttribute DataObj_T = (DataAttribute)DataObj.GetChild("t");

			iedServer.UpdateFloatAttributeValue(DataObj_F, float.Parse(value));
			iedServer.UpdateTimestampAttributeValue(DataObj_T, new Timestamp(DateTime.Now));
		}

		public void ModifySpsValue(string ObjRef, string value)
		{
			DataObject DataObj = (DataObject)iedModel.GetModelNodeByShortObjectReference(ObjRef);

			DataAttribute DataObj_ST = (DataAttribute)DataObj.GetChild("stVal");
			DataAttribute DataObj_T = (DataAttribute)DataObj.GetChild("t");

			iedServer.UpdateBooleanAttributeValue(DataObj_ST, Convert.ToBoolean(value));
			iedServer.UpdateTimestampAttributeValue(DataObj_T, new Timestamp(DateTime.Now));
		}

		public void SetControlListener(string ObjRef)
		{
			DataObject ControlPoint = (DataObject)iedModel.GetModelNodeByShortObjectReference(ObjRef);
			iedServer.SetCheckHandler(ControlPoint, delegate (ControlAction action, object parameter, MmsValue ctlVal, bool test, bool interlockCheck) {

				Console.WriteLine("SYSLOG: Received binary control command:");
				Console.WriteLine("   ctlNum: " + action.GetCtlNum());
				Console.WriteLine("   execution-time: " + action.GetControlTimeAsDataTimeOffset().ToString());

				return CheckHandlerResult.ACCEPTED;
			}, null);

			iedServer.SetControlHandler(ControlPoint, delegate (ControlAction action, object parameter, MmsValue ctlVal, bool test) {
				bool val = ctlVal.GetBoolean();

				if (val)
					Console.WriteLine("CTRL: Execute binary control command: ON");
				else
					Console.WriteLine("CTRL: Execute binary control command: OFF");

				return ControlHandlerResult.OK;
			}, null);
		}

		public void Stop()
		{
			iedServer.Stop();
			Console.WriteLine("SYSLOG: Iec61850 Server is stopped");

			iedServer.Destroy();
		}

		/*private static void gooseListener(GooseSubscriber subscriber, object parameter)
		{
			Console.WriteLine("Received GOOSE message:\n-------------------------");

			Console.WriteLine("  stNum: " + subscriber.GetStNum());

			Console.WriteLine("  sqNum: " + subscriber.GetSqNum());


			MmsValue values = subscriber.GetDataSetValues();

			Console.WriteLine("  values: " + values.Size().ToString());

			foreach (MmsValue value in values)
			{
				Console.WriteLine("   value: " + value.ToString());
			}
		}*/
	}
}
