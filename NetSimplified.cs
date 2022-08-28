using System.IO;
using Terraria.ModLoader;

namespace NetSimplified
{
	/// <summary>
	/// �ֱ��� <see cref="Mod.HandlePacket(BinaryReader, int)"/> �� <see cref="Mod.Unload"/> �������е����� <see langword="static"/> ����
	/// </summary>
	public static class NetSimplified
	{
		/// <summary>�����������Mod�ķ���, ���� <see cref="Mod.HandlePacket(BinaryReader, int)"/> ����</summary>
		public static void HandleModule(BinaryReader reader, int whoAmI) {
			NetModule.ReceiveModule(reader, whoAmI);
		}

		/// <summary>Modж��ʱж�ز�����ֵ��ֶ�, �Ի����ڴ�, ���� <see cref="Mod.Unload"/> �� <see cref="ModSystem.OnModUnload"/> ����</summary>
		public static void Unload() {
			NetModule.UnloadDictionaries();
		}
	}
}