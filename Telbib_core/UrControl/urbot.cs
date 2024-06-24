using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

interface IEncoderDecoder
{
	object Decode(ref object o, byte[] buf, ref int offset);
	void Encode(object o,byte[] buf, ref int offset);
}

class EncodeValue: IEncoderDecoder // For bool, uint, int, ulong, double
{
	Type type;
	int Typesize;

	public EncodeValue(Type type)
	{
		this.type = type;
		Typesize = Marshal.SizeOf(type);
	}
	
	public void Encode(object o, byte[] buf, ref int offset)
	{

		byte[] b=null;
		switch (type.FullName)
		{
			case "System.Boolean":
				b = BitConverter.GetBytes((bool)o);
				break;
			case "System.Byte":
				b = new byte[1];
				b[0] = (byte)o;
				break;
			case "System.UInt32":
				b = BitConverter.GetBytes((UInt32)o);
				break;
			case "System.Int32":
				b = BitConverter.GetBytes((Int32)o);
				break;
			case "System.UInt64":
				b = BitConverter.GetBytes((UInt64)o);
				break;
			case "System.Double":
				b = BitConverter.GetBytes((Double)o);
				break;
		}

		if (BitConverter.IsLittleEndian)
			Array.Reverse(b);
		Array.Copy(b, 0, buf, offset, Typesize);
		offset += Typesize;
	}
	
	public object Decode(ref object o, byte[] buf, ref int offset)
	{

		// object o not used, value type

		byte[] b = new byte[Typesize];
		Array.Copy(buf, offset, b, 0, Typesize);
		if (BitConverter.IsLittleEndian)
			Array.Reverse(b);
		offset += Typesize;

		switch (type.FullName)
		{
			case "System.Boolean":
				return BitConverter.ToBoolean(b, 0);
			case "System.Byte":
				return b[0];
			case "System.UInt32":
				return BitConverter.ToUInt32(b, 0);
			case "System.Int32":
				return BitConverter.ToInt32(b, 0);
			case "System.UInt64":
				return BitConverter.ToUInt64(b, 0);
			case "System.Double":
				return BitConverter.ToDouble(b, 0);
		}

		return null;             
	}
}
class EncodeArray : IEncoderDecoder // For uint[], int[], ulong[], double[]
{
	int ArraySize, Typesize;
	Type type;
	public EncodeArray(int size, Type type)
	{
		ArraySize = size;
		Typesize = Marshal.SizeOf(type);
		this.type = type;
	}
	
	public void Encode(object o, byte[] buf, ref int offset)
	{
		Array array = o as Array;

		for (int i = 0; i < ArraySize; i++)
		{
			byte[] b=null;

			switch (type.FullName)
			{
				case "System.UInt32":
					b = BitConverter.GetBytes((UInt32)array.GetValue(i));
					break;
				case "System.Int32":
					b = BitConverter.GetBytes((Int32)array.GetValue(i));
					break;
				case "System.UInt64":
					b = BitConverter.GetBytes((UInt64)array.GetValue(i));
					break;
				case "System.Double":
					b = BitConverter.GetBytes((Double)array.GetValue(i));
					break;
			}
			if (BitConverter.IsLittleEndian)
				Array.Reverse(b);
			Array.Copy(b, 0, buf, offset, Typesize);
			offset += Typesize;
		}
	}
	public object Decode(ref object o, byte[] buf, ref int offset)
	{

		Array obj = o as Array;

		for (int i = 0; i < ArraySize; i++)
		{
			byte[] b = new byte[Typesize];
			Array.Copy(buf, offset, b, 0, Typesize);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(b);
			offset += Typesize;

			object value = null; ;

			switch (type.FullName)
			{
				case "System.UInt32":
					value = BitConverter.ToUInt32(b, 0);
					break;
				case "System.Int32":
					value = BitConverter.ToInt32(b, 0);
					break;
				case "System.UInt64":
					value = BitConverter.ToUInt64(b, 0);
					break;
				case "System.Double":
					value = BitConverter.ToDouble(b, 0);
					break;
			}

			obj.SetValue(value,i);
		}

		return obj; // Not used, type reference
	}
}

static class Constants
{
	public const string ROBOT_IP = "192.168.0.102"; // Real Robot Wifi(UR3_wifi)
	//public const string ROBOT_IP = "192.168.56.101"; // URSim
	public const int RECIVE_FREQ = 72; // 14 ms(freq of the VR set), take care BLOCKING(See procces monitor)
	public const double ROTY_OFFSET = -1.57;
	public const double POSZ_OFFSET = -0.6;
	
}

/*

See :
https://www.universal-robots.com/how-tos-and-faqs/how-to/ur-how-tos/real-time-data-exchange-rtde-guide-22229/ 
 
BOOL : bool
UINT8 : byte
UINT32 : uint
UINT64 : ulong
INT32 : int
DOUBLE : double
VECTOR3D : double[]
VECTOR6D : double []
VECTOR6INT32 : int[]
VECTOR6UINT32 : uint[]
  
TODO and not TODO : do not declare public fields with other types & creates the array with the right size

*/
[Serializable]
public class UniversalRobot_Outputs
{
	// public double io_current; // check the fields name in the RTDE guide : MUST be the same with the same type
	public double[] actual_q = new double[6]; // array creation must be done here to give the size
	public double[] actual_TCP_pose = new double[6]; // Tcp pose
	public double timestamp;
	public double output_double_register_0; 
	public double output_double_register_1; 
	public double output_double_register_2; 
	// public int robot_mode;
	// free private & protected attributs are allows
	// all properties and methods also (even public)
}

[Serializable]
public class UniversalRobot_Inputs
{
	public byte tool_digital_output_mask;
	public byte tool_digital_output;
	public double input_double_register_24;
	public double input_double_register_0; 
	public double input_double_register_1; 
	public double input_double_register_2; 
	public double input_double_register_3; 
	public double input_double_register_4; 
	public double input_double_register_5; 
	public int input_int_register_25; 
}

public partial class urbot : Node3D 
{
	Node3D hand;
	static UniversalRobot_Outputs UrOutputs=new UniversalRobot_Outputs();
	static UniversalRobot_Inputs UrInputs=new UniversalRobot_Inputs();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		 hand = GetNodeOrNull<Node3D>("/root/Node3D/XROrigin3D/RightHand");
		
		// Connection using the protocol version 2 (allows update frequency less or equal to 125 Hz)
		
		OnSockClosed += new EventHandler(Ur3_OnSockClosed);

		GD.Print("Connected: " + Connect(Constants.ROBOT_IP,2));

		// Register Inputs (UR point of view)
		UrInputs.input_int_register_25 = 1;
		GD.Print("inputs: " + Setup_Ur_Inputs(UrInputs));


		// Register Outputs (UR point of view), for an update frequency 10Hz
		GD.Print(Setup_Ur_Outputs(UrOutputs, Constants.RECIVE_FREQ));
		OnDataReceive += new EventHandler(Ur3_OnDataReceive); 

		// Request the UR to send back Outputs periodically
		Ur_ControlStart();


		// Bye bye Robot
		//Ur3.Disconnect(); 
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//GD.Print("pos"+hand.GlobalPosition + " rot " + hand.GlobalRotation);
		
		// Robotx mapping(Axis Z in VR)
		double robot_rotx = hand.GlobalRotation[2];
		double robot_posx = hand.GlobalPosition[2];
		//if ( robot_rotx < -0.45) robot_rotx = -0.45;
		//else if (robot_rotx > 0.45) robot_rotx = 0.45;
		
		// Roboty mapping(Axis X in VR)
		double robot_roty = hand.GlobalRotation[0];
		double robot_posy = hand.GlobalPosition[0];
		//if ( robot_posy < -0.37) robot_posy = -0.37;
		//else if (robot_posy > 0.37) robot_posy = 0.37;
		
		
		// Robotz mapping(Axis Y in VR)
		double robot_rotz = hand.GlobalRotation[1];
		double robot_posz = hand.GlobalPosition[1]; 
		//if ( robot_posz < 0.1) robot_posz = 0.100;
		//else if (robot_posz> 0.65) robot_posz = 0.65;
		
		UrInputs.input_double_register_0 = robot_posx;                
		UrInputs.input_double_register_1 = robot_posy;
		UrInputs.input_double_register_2 = robot_posz + Constants.POSZ_OFFSET;
		UrInputs.input_double_register_3 = robot_rotx;
		UrInputs.input_double_register_4 = robot_roty + Constants.ROTY_OFFSET;
		UrInputs.input_double_register_5 = robot_rotz;
		
		// Send data to UR3
		GD.Print("send inputs:" + Send_Ur_Inputs());
		 
	}
	
	// Return the complete pose of the TCP (to be used in gd external scripts)
	 public double[] GetActualTCPPose()
	{
		return UrOutputs.actual_TCP_pose;
	}
	
	// Return the rotation of TCP in RPY format(need to be implemented in the URscript)
	 public double[] GetActualRPYRot()
	{
		double[] RPY = new double[3];
		RPY[0] = UrOutputs.output_double_register_0;
		RPY[1] = UrOutputs.output_double_register_1;
		RPY[2] = UrOutputs.output_double_register_2;
		
		return RPY;
	}

	static void Ur3_OnSockClosed(object sender, EventArgs e)
	{
		GD.Print("Closed");
	}

	// Recive UR3 data and print it
	static void Ur3_OnDataReceive(object sender, EventArgs e)
	{
		GD.Print("TCP pose recived: x--", UrOutputs.actual_TCP_pose[0],
				 " y-- ", UrOutputs.actual_TCP_pose[1],
				 " z-- ", UrOutputs.actual_TCP_pose[2],
				 " rotx-- ", UrOutputs.actual_TCP_pose[3],
				 " roty-- ", UrOutputs.actual_TCP_pose[4],
				 " rotz-- ", UrOutputs.actual_TCP_pose[5]);
	}	
	enum RTDE_Command
	{
		REQUEST_PROTOCOL_VERSION = 86,
		GET_URCONTROL_VERSION = 118,
		TEXT_MESSAGE = 77,
		DATA_PACKAGE = 85,
		CONTROL_PACKAGE_SETUP_OUTPUTS = 79,
		CONTROL_PACKAGE_SETUP_INPUTS = 73,
		CONTROL_PACKAGE_START = 83,
		CONTROL_PACKAGE_PAUSE = 80
	};

	int TimeOut = 500;
	TcpClient sock = new TcpClient();
	ManualResetEvent receiveDone = new ManualResetEvent(false);

	public uint ProtocolVersion { get; private set; }

	byte[] bufRecv = new byte[1500]; // Enough to hold a full OK CONTROL_PACKAGE_SETUP_OUTPUTS response

	public event EventHandler OnDataReceive;
	public event EventHandler OnSockClosed;

	byte Outputs_Recipe_Id, Inputs_Recipe_Id; // from the Robot point of view

	object UrStructOuput, UrStructInput;

	IEncoderDecoder[] UrStructOuputDecoder, UrStructInputDecoder;

	public String ErrorMessage { get; private set; }

	public bool Connect(String host, uint ProtocolVersion=2, int timeOut = 500)
	{
		byte[] InternalbufRecv = new byte[bufRecv.Length];
		TimeOut = timeOut;
		this.ProtocolVersion = 1;

		try
		{
			sock.Connect(host, 30004);
			sock.Client.BeginReceive(InternalbufRecv, 0, InternalbufRecv.Length, SocketFlags.None, AsynchReceive, InternalbufRecv);

			if (ProtocolVersion != 1)
				Set_UR_Protocol_Version(ProtocolVersion);

			return true;
		}
		catch { return false; }
	}

	private void AsynchReceive(IAsyncResult ar)
	{
		int bytesRead = sock.Client.EndReceive(ar);
		byte[] InternalbufRecv = (byte[])ar.AsyncState;

		if (bytesRead > 0)
		{

			lock (bufRecv)
				Array.Copy(InternalbufRecv, bufRecv, InternalbufRecv.Length);

			if (InternalbufRecv[2] == (byte)RTDE_Command.TEXT_MESSAGE)
			{
				if (ProtocolVersion==1)
					ErrorMessage = Encoding.ASCII.GetString(InternalbufRecv, 4, InternalbufRecv[1]-4-2); // try catch not required
				else
					ErrorMessage = Encoding.ASCII.GetString(InternalbufRecv, 4, InternalbufRecv[3]); // try catch not required
			}

			receiveDone.Set();

			sock.Client.BeginReceive(InternalbufRecv, 0, InternalbufRecv.Length, SocketFlags.None, AsynchReceive, InternalbufRecv);

			try
			{
				if (bufRecv[2] == (byte)RTDE_Command.DATA_PACKAGE)
				{
					int offset = 3;
					if (ProtocolVersion == 2)
					{
						offset++;
						if (bufRecv[3] != Outputs_Recipe_Id) return;
					}

					FieldInfo[] f = UrStructOuput.GetType().GetFields();

					for (int i = 0; i < f.Length; i++)
					{
						object currentvalue = f[i].GetValue(UrStructOuput);

						if (f[i].FieldType.IsArray)
							UrStructOuputDecoder[i].Decode(ref currentvalue, bufRecv, ref offset); // value type
						else
							f[i].SetValue(UrStructOuput, UrStructOuputDecoder[i].Decode(ref currentvalue, bufRecv, ref offset));
					}

					if (OnDataReceive != null)
						OnDataReceive(this, null);
				}
			}
			catch {}
		}
		else
			if (OnSockClosed!=null)
				OnSockClosed(this, null);
	}

	public void Disconnect()
	{
		sock.Close();
	}

	private void SendRtdePacket(RTDE_Command RTDEType, byte[] payload=null)
	{
		ErrorMessage = null;

		if (payload==null) payload=new byte[0];

		byte[] s = new byte[payload.Length + 3];

		byte[] size=BitConverter.GetBytes(payload.Length + 3);

		s[0] = size[1];
		s[1] = size[0];
		s[2] = (byte)RTDEType;

		if (payload != null)
			Array.Copy(payload, 0, s, 3, payload.Length);

		receiveDone.Reset();
		sock.Client.BeginSend(s, 0, s.Length, SocketFlags.None, null, null); // not Send() to be thread safe with the BeginReceive
	   
	}

	private bool Send_UR_Command(RTDE_Command Cmd, byte[] payload=null)
	{
		SendRtdePacket(Cmd, payload);
		if (receiveDone.WaitOne(TimeOut))
		{
			lock (bufRecv)
			{                    
				return (bufRecv[2] == (byte)Cmd);
			}
		}
		return false;
	}

	private bool Set_UR_Protocol_Version(uint Version)
	{
		byte[] V={ 0, (byte)Version};

		bool ret= Send_UR_Command(RTDE_Command.REQUEST_PROTOCOL_VERSION, V);

		if ((ret == true)&&(bufRecv[3]==1)) ProtocolVersion = Version;

		return ret;
	}

	public bool Ur_ControlStart()
	{
		return Send_UR_Command(RTDE_Command.CONTROL_PACKAGE_START);
	}

	public bool Ur_ControlPause()
	{
		return Send_UR_Command(RTDE_Command.CONTROL_PACKAGE_PAUSE);
	}

	public bool Send_Ur_Inputs()
	{         
		FieldInfo[] f = UrStructInput.GetType().GetFields();

		byte[] buf = new byte[1500];
		int offset = 0;

		for (int i = 0; i < f.Length; i++)
			UrStructInputDecoder[i].Encode(f[i].GetValue(UrStructInput), buf, ref offset);

		byte[] payload;

		if (ProtocolVersion==1)
		{
			payload = new byte[offset];
			Array.Copy(buf,payload,offset);
		}
		else
		{
			payload = new byte[offset+1];
			payload[0] = Inputs_Recipe_Id;
			Array.Copy(buf, 0, payload, 1, offset);
		}

		Send_UR_Command(RTDE_Command.DATA_PACKAGE, payload);

		return true;
	}
	private bool Setup_Ur_InputsOutputs(RTDE_Command Cmd, object UrStruct, out IEncoderDecoder[] encoder, double Frequency = 1)
	{
		// Get the public fields in the structure 
		FieldInfo[] f = UrStruct.GetType().GetFields();
		encoder = new IEncoderDecoder[f.Length];

		StringBuilder b = new StringBuilder();
		for (int i = 0; i < f.Length; i++)
		{
			b.Append((i == 0 ? "" : ",") + f[i].Name); // build the RTDE request : names and comma

			if (f[i].FieldType.IsArray) // link to the encoder/decoder
			{
				Array array = f[i].GetValue(UrStruct) as Array;
				object element = array.GetValue(0);
				encoder[i] = new EncodeArray(array.Length, element.GetType());
			}
			else
				encoder[i] = new EncodeValue(f[i].FieldType);
		}

		byte[] payload;
		if ((Cmd == RTDE_Command.CONTROL_PACKAGE_SETUP_OUTPUTS) && (ProtocolVersion == 2))
		{
			payload = new byte[b.Length + 8];

			byte[] Freq = BitConverter.GetBytes(Frequency);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(Freq);
			Array.Copy(Freq, 0, payload, 0, 8);
			Array.Copy(Encoding.ASCII.GetBytes(b.ToString()), 0, payload, 8, b.Length);
		}
		else
			payload=Encoding.ASCII.GetBytes(b.ToString());

		if (Send_UR_Command(Cmd, payload) == true)
		{
			if (Cmd == RTDE_Command.CONTROL_PACKAGE_SETUP_OUTPUTS)
				Outputs_Recipe_Id = bufRecv[3]; // only for Protocol Version 2
			else
				Inputs_Recipe_Id = bufRecv[3]; // only for Protocol Version 2

			String s = Encoding.ASCII.GetString(bufRecv, 3, bufRecv.Length - 3);
			GD.Print("received: " + s);
			if (s.Contains("NOT_FOUND")) return false;
			if (s.Contains("IN_USE")) return false;

			return true;
		}
		return false;
	}

	public bool Setup_Ur_Outputs(object UrStruct, double Frequency=1)
	{
		this.UrStructOuput = UrStruct;
		return Setup_Ur_InputsOutputs(RTDE_Command.CONTROL_PACKAGE_SETUP_OUTPUTS, UrStruct, out UrStructOuputDecoder, Frequency);
	}

	public bool Setup_Ur_Inputs(object UrStruct)
	{
		this.UrStructInput = UrStruct;
		return Setup_Ur_InputsOutputs(RTDE_Command.CONTROL_PACKAGE_SETUP_INPUTS, UrStruct, out UrStructInputDecoder);
	}
}
