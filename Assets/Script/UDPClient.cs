using System;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace AssemblyCSharp
{
	public interface StatusTranslator
	{
		UInt32 Type ();

		object Translate (NetworkReader reader);
	}


	public interface OperationGenerator
	{
		UInt32 Type ();

		byte[] Generate (object obj);
	}

	public class Operation
	{
		public UInt32 Type;
		public object Object;

		public Operation (UInt32 t, object obj)
		{
			this.Type = t;
			this.Object = obj;
		}
	}

	public class Status
	{
		public UInt32 Type;
		public object Object;

		public Status (UInt32 t, object obj)
		{
			this.Type = t;
			this.Object = obj;
		}
	}

	public class UDPClient
	{
		public delegate void Recv (Status s);

		Dictionary<UInt32,StatusTranslator> translators;
		Dictionary<UInt32,OperationGenerator> generators;
		Socket socket;
		EndPoint remote;
		Recv recv;
		int maxBufferSize;
		bool closed;



		public UDPClient (string ip, int port, int maxBufferSize, int closeTimeout, Recv recv)
		{
			this.socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			this.socket.Bind (new IPEndPoint (IPAddress.Any, 0));
			this.remote = new IPEndPoint (IPAddress.Parse (ip), port);
			this.maxBufferSize = maxBufferSize;
			this.translators = new Dictionary<uint, StatusTranslator> ();
			this.generators = new Dictionary<uint, OperationGenerator> ();
			this.recv = recv;
			this.closed = false;

			this.socket.ReceiveTimeout = closeTimeout;//毫秒
		}



		public void AddStatusTranslator (StatusTranslator t)
		{
			this.translators [t.Type ()] = t;
		}

		public void AddOperationGenerator (OperationGenerator g)
		{
			this.generators [g.Type ()] = g;
		}

		public void Send (Operation[] ops)
		{
			if (this.closed) {
				return;
			}
			var opsData = new byte[ops.Length][];
			var length = 0;
			for (var i = 0; i < ops.Length; i++) {
				var op = ops [i];
				var g = this.generators [op.Type];
				if (g != null) {
					var d = g.Generate (op.Object);
					if (d != null) {
						var w = new NetworkWriter ();
						w.WriteUInt32 (op.Type);
						w.WriteBytes (d);
						opsData [i] = w.Buffer ();
						length += w.Length ();
					}
				}
			}

			var data = new byte[this.maxBufferSize];
			var dataLength = 0;
			foreach (var d in opsData) {
				if (dataLength + d.Length > this.maxBufferSize) {
					//数据即将超过缓冲区大小时需要发送并清空缓冲区
					this.socket.SendTo (data, dataLength, SocketFlags.None, remote);
					dataLength = 0;
				}
				d.CopyTo (data, dataLength);
				dataLength += d.Length;
			}
			if (dataLength > 0) {
				this.socket.SendTo (data, dataLength, SocketFlags.None, remote);
			}
		}

		public void Start ()
		{
			new Thread (() => {
				//循环接收消息
				var buffer = new byte[this.maxBufferSize];
				while (!this.closed) {
					try {
						var count = this.socket.ReceiveFrom (buffer, ref this.remote);
						if (count > 0) {
							var reader = new NetworkReader (buffer, count);
							while (reader.Length () > 0) {
								var t = reader.ReadUInt32 ();
								if (t == null) {
									break;
								}
								var trans = this.translators [t.Value];
								if (trans != null) {
									var obj = trans.Translate (reader);
									if (obj != null) {
										this.recv (new Status (t.Value, obj));
									}
								}

							}
						}
					} catch {
						this.Close ();
					}
				}
			}).Start ();
		}

		public void Close ()
		{
			if (!this.closed) {
				this.socket.Close ();
				this.closed = true;
			}

		}

	}
}

