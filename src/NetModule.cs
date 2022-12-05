﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NetSimplified.Syncing;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NetSimplified
{
    /// <summary>
    /// 用于写入、读取 <see cref="ModPacket"/> 的基类
    /// </summary>
    public abstract class NetModule : ModType
    {
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

        public sealed override void SetupContent() => SetStaticDefaults();

        protected sealed override void Register() {
            NetModuleLoader.Register(this);
        }

        protected sealed override void ValidateType() {
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Static |
                                             BindingFlags.NonPublic | BindingFlags.Public);
            if (fields.Any(f =>
                    !AutoSyncHandler.SupportedTypes.Contains(f.FieldType) &&
                    Attribute.IsDefined(f, typeof(AutoSyncAttribute)))) {
                throw new NotSupportedException("字段不支持自动传输");
            }
        }

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

        /// <summary>包的发送者</summary>
        protected int Sender { get; private set; } = Main.myPlayer;

        /// <summary>该 <see cref="NetModule"/> 被分配到的ID</summary>
        public int Type { get; internal set; }

        /// <summary>
        /// 使用这个函数来自行发送字段
        /// </summary>
        /// <param name="p">用于发包的 <see cref="ModPacket"/> 实例</param>
        public virtual void Send(ModPacket p) {
        }

        /// <summary>
        /// 通过 <see cref="ModPacket"/> 发包
        /// </summary>
        /// <param name="toClient">如果不是 -1, 则包<b>只会</b>发送给对应的客户端</param>
        /// <param name="ignoreClient">如果不是 -1, 则包<b>不会</b>发送给对应的客户端</param>
        /// <param name="runLocally">如果为 <see langword="true"/> 则在发包时会调用 <see cref="Receive()"/> 方法</param>
        public void Send(int toClient = -1, int ignoreClient = -1, bool runLocally = false) {
            if (PreSend(toClient, ignoreClient)) {
                if (Main.netMode != NetmodeID.SinglePlayer) {
                    ModPacket mp = Mod.GetPacket();
                    mp.Write(Type);
                    AutoSyncHandler.HandleAutoSend(this, mp);
                    Send(mp);
                    mp.Send(toClient, ignoreClient);
                }

                if (runLocally) {
                    Receive();
                }
            }
        }

        /// <summary>
        /// 使用这个函数来自行读取字段
        /// </summary>
        /// <param name="r">用于读取的 <see cref="BinaryReader"/> 实例</param>
        public virtual void Read(BinaryReader r) {
        }

        /// <summary>
        /// 使用这个函数来进行接收后的操作 (与 <see cref="Read(BinaryReader)"/> 分开以适配 runLocally)
        /// </summary>
        public abstract void Receive();

        /// <summary>发包前调用, 返回 <see langword="false"/> 则不会发包, 也不会调用 <see cref="Receive()"/>。 默认为 <see langword="true"/>.</summary>
        protected virtual bool PreSend(int toClient = -1, int ignoreClient = -1) => true;

        /// <summary>接收来自你的Mod的发包, 请在 <see cref="Mod.HandlePacket(BinaryReader, int)"/> 调用</summary>
        public static void ReceiveModule(BinaryReader reader, int whoAmI) {
            var module = NetModuleLoader.Get(reader.ReadInt32());
            module.Sender = whoAmI;
            AutoSyncHandler.HandleAutoRead(module, reader);
            module.Read(reader);
            module.Receive();
        }
    }
}