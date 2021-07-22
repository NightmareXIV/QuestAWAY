using Dalamud.Game.Internal;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuestAWAY
{
    unsafe class QuestAWAY : IDalamudPlugin
    {
        public string Name => "QuestAWAY!";
        internal DalamudPluginInterface pi;
        ByteColor transparent = new ByteColor()
        {
            A = 0, B = 0, G = 0, R = 0
        };

        public void Dispose()
        {
            pi.Framework.OnUpdateEvent -= Tick;
            pi.Dispose();
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            pi = pluginInterface;
            pi.Framework.OnUpdateEvent += Tick;
        }

        [HandleProcessCorruptedStateExceptions]
        public void Tick(object _)
        {
            try
            {
                var o = pi.Framework.Gui.GetUiObjectByName("AreaMap", 1);
                if (o == IntPtr.Zero)
                {
                    return;
                }
                var masterWindow = (AtkUnitBase*)o;
                if (!masterWindow->IsVisible) return;
                var mapCNode = (AtkComponentNode*)masterWindow->UldManager.NodeList[3];
                for (var i = 6; i < mapCNode->Component->UldManager.NodeListCount; i++)
                {
                    var mapIconNode = (AtkComponentNode*)mapCNode->Component->UldManager.NodeList[i];
                    var imageNode = (AtkImageNode*)mapIconNode->Component->UldManager.NodeList[4];
                    var textureInfo = imageNode->PartsList->Parts[imageNode->PartId].UldAsset;
                    var texType = textureInfo->AtkTexture.TextureType;
                    if (texType == TextureType.Resource)
                    {
                        var texFileNamePtr = textureInfo->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle.FileName;
                        var texString = Marshal.PtrToStringAnsi(new IntPtr(texFileNamePtr));
                        if (texString == "ui/icon/071000/071021.tex")
                        {
                            mapIconNode->AtkResNode.Color = transparent;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                pi.Framework.Gui.Chat.Print(e.Message);
            }
        }
    }
}
