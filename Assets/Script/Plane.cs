using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using AssemblyCSharp;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System;


public class Direction
{
	public float x;
	public float y;
}

public class Position
{
	public float x;
	public float y;
}

public class PositionStatusTranslator : StatusTranslator
{
	public UInt32 Type ()
	{
		return 1;
	}

	public object Translate (NetworkReader reader)
	{
		var x = reader.ReadFloat ();
		var y = reader.ReadFloat ();
		return new Position (){ x = x.Value, y = y.Value };
	}
}

public class DirectionOperationGenerator : OperationGenerator
{
	public UInt32 Type ()
	{
		return 1;
	}

	public byte[] Generate (object obj)
	{
		var s = obj as Direction;
		var w = new NetworkWriter ();
		w.WriteFloat (s.x);
		w.WriteFloat (s.y);
		return w.Buffer ();
	}
}

public class Plane : MonoBehaviour
{
	UDPClient client;
	Position pos;

	// Use this for initialization
	void Start ()
	{
		this.pos = new Position ();
		this.client = new UDPClient ("127.0.0.1", 10086, 1024, 1000, this.UpdatePos);
		this.client.AddOperationGenerator (new DirectionOperationGenerator ());
		this.client.AddStatusTranslator (new PositionStatusTranslator ());
		this.client.Start ();
	}

	void UpdatePos (Status s)
	{
		var o = s.Object as Position;
		this.pos = o;
		//Debug.Log (s.Type);
		//Debug.Log (o.x);
		//Debug.Log (o.y);
	}

	void Update ()
	{
		var p = this.transform.position;
		p.x = this.pos.x;
		p.y = this.pos.y;
		this.transform.position = p;

		//更新位置
		var d = new Direction ();
		if (Input.GetKey (KeyCode.W)) {
			d.y += 1;
		}
		if (Input.GetKey (KeyCode.S)) {
			d.y -= 1;
		}
		if (Input.GetKey (KeyCode.A)) {
			d.x -= 1;
		}
		if (Input.GetKey (KeyCode.D)) {
			d.x += 1;
		}
		var op = new Operation (1, d);
		this.client.Send (new Operation[]{ op });
	}


	void OnDestory ()
	{
		client.Close ();
	}

	void OnApplicationQuit ()
	{
		client.Close ();
	}
}
