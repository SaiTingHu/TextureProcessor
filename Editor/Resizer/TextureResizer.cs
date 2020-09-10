using System;
using System.Collections.Generic;
using UnityEditor;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理缩放器
    /// </summary>
    public sealed class TextureResizer : IDisposable
    {
        internal List<TextureResizeAgent> Agents;

        /// <summary>
        /// 生成一个纹理缩放器，处理整个项目中的所有纹理
        /// </summary>
        public TextureResizer()
        {
            Agents = Utility.CreataResizeAgents(Utility.FindTextures(new string[] { "Assets" }));
        }

        /// <summary>
        /// 生成一个纹理缩放器，处理指定文件夹下的所有纹理
        /// </summary>
        /// <param name="folders">文件夹路径</param>
        public TextureResizer(string[] folders)
        {
            Agents = Utility.CreataResizeAgents(Utility.FindTextures(folders));
        }
        
        /// <summary>
        /// 缩放纹理尺寸为4的倍数
        /// </summary>
        /// <returns>处理的反馈信息</returns>
        public List<TextrueResizeFeedback> ResizeToMultipleOf4()
        {
            List<TextrueResizeFeedback> feedbacks = new List<TextrueResizeFeedback>();
            for (int i = 0; i < Agents.Count; i++)
            {
                TextrueResizeFeedback feedback = Agents[i].ResizeToMultipleOf4();
                if (feedback != null)
                {
                    feedbacks.Add(feedback);
                }
                
                if (EditorUtility.DisplayCancelableProgressBar("Textrue Resize", Agents[i].Path, (float)i / Agents.Count))
                {
                    break;
                }
            }
            EditorUtility.ClearProgressBar();
            return feedbacks;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            if (Agents != null)
            {
                for (int i = 0; i < Agents.Count; i++)
                {
                    Agents[i].Dispose();
                }
                Agents = null;
            }
        }
    }
}