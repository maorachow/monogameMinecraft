using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Runtime.CompilerServices;
 
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace monogameMinecraft
{
    public class GameOptions
    {
        public static string path = AppDomain.CurrentDomain.BaseDirectory;
        public static int renderDistance = 512;
        public static bool renderShadow = false;
        public static bool renderFarShadow = false;
        public static bool renderSSAO=false;
        public static bool renderLightShaft = false;
        public static void ReadOptionsJson()
        {
            if (!Directory.Exists(path + "unityMinecraftServerData"))
            {
                Directory.CreateDirectory(path + "unityMinecraftServerData");

            }
           

            if (!File.Exists(path + "unityMinecraftServerData" + "/options.json"))
            {
                FileStream fs = File.Create(path + "unityMinecraftServerData" + "/options.json");
                fs.Close();
            }
           
            string data = File.ReadAllText(path + "unityMinecraftServerData/options.json");
            if(data.Length> 0)
            {
                try{
             GameOptionsData dataOptions=JsonSerializer.Deserialize<GameOptionsData>(data);
            renderDistance = dataOptions.renderDistance;
            renderShadow = dataOptions.renderShadow;
            renderFarShadow = dataOptions.renderFarShadow;
                    renderSSAO= dataOptions.renderSSAO;
                    renderLightShaft= dataOptions.renderLightShaft;
                }catch(Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
           
            }
            
        }

        public static void SaveOptions(object obj)
        {

            FileStream fs;
            if (File.Exists(path + "unityMinecraftServerData/options.json"))
            {
                fs = new FileStream(path + "unityMinecraftServerData/options.json", FileMode.Truncate, FileAccess.Write);//Truncate模式打开文件可以清空。
            }
            else
            {
                fs = new FileStream(path + "unityMinecraftServerData/options.json", FileMode.Create, FileAccess.Write);
            }
            fs.Close();
            

          
            GameOptionsData data=new GameOptionsData(GameOptions.renderDistance, GameOptions.renderShadow, GameOptions.renderFarShadow,GameOptions.renderSSAO,GameOptions.renderLightShaft);
            string dataSerialized = JsonSerializer.Serialize<GameOptionsData>(data);
            File.WriteAllText(path + "unityMinecraftServerData/options.json", dataSerialized);
            
        }
        public static void ChangeRenderDistance(UIButton obj)
        {
            obj.text = "Render Distance : " + renderDistance;
            renderDistance += 32;
            if (renderDistance >= 544)
            {
                renderDistance = 64;
            }
            obj.text = "Render Distance : "+renderDistance;
        }
        public static void ChangeRenderShadow(UIButton obj)
        {
            obj.text = "Render Shadow : " + renderShadow.ToString();
            renderShadow = !renderShadow;
            obj.text="Render Shadow : "+renderShadow.ToString();
        }
        public static void ChangeRenderFarShadow(UIButton obj)
        {

            obj.text = "Render Far Shadow : " + renderFarShadow.ToString();
            renderFarShadow = !renderFarShadow;
            obj.text = "Render Far Shadow : " + renderFarShadow.ToString();
        }
        public static void ChangeRenderSSAO(UIButton obj)
        {

            obj.text = "Render SSAO : " + renderSSAO.ToString();
            renderSSAO = !renderSSAO;
            obj.text = "Render SSAO : " + renderSSAO.ToString();
        }
        public static void ChangeRenderLightShaft(UIButton obj)
        {

            obj.text = "Render Light Shaft : " + renderLightShaft.ToString();
            renderLightShaft = !renderLightShaft;
            obj.text = "Render Light Shaft : " + renderLightShaft.ToString();
        }
    }

    public class GameOptionsData
    {
        [JsonInclude]
        public int renderDistance;
        [JsonInclude]
        public bool renderShadow;
        [JsonInclude]
        public bool renderFarShadow;
        [JsonInclude]
        public bool renderSSAO;
        [JsonInclude]
        public bool renderLightShaft;
        public GameOptionsData(int renderDistance,bool renderShadow,bool renderFarShadow,bool renderSSAO,bool renderLightShaft)
        {
            this.renderDistance = renderDistance;
            this.renderShadow = renderShadow;
            this.renderFarShadow = renderFarShadow;
            this.renderSSAO= renderSSAO;
            this.renderLightShaft = renderLightShaft;
        }
    }
}
