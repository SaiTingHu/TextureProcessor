using System.Collections.Generic;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理处理器
    /// </summary>
    public sealed class TextureProcessor
    {
        private List<TextureAgent> _textureAgents;

        /// <summary>
        /// 生成一个纹理处理器，处理整个项目中的所有纹理
        /// </summary>
        public TextureProcessor()
        {
            _textureAgents = Utility.CreataAgents(Utility.FindTextures(new string[] { "Assets" }));
        }

        /// <summary>
        /// 生成一个纹理处理器，处理指定文件夹下的所有纹理
        /// </summary>
        /// <param name="folders">文件夹路径</param>
        public TextureProcessor(string[] folders)
        {
            _textureAgents = Utility.CreataAgents(Utility.FindTextures(folders));
        }

        /// <summary>
        /// 修正纹理尺寸为4的倍数
        /// </summary>
        /// <returns>处理的反馈信息</returns>
        public List<TextrueProcessedFeedback> ResizeToMultipleOf4()
        {
            List<TextrueProcessedFeedback> feedbacks = new List<TextrueProcessedFeedback>();
            for (int i = 0; i < _textureAgents.Count; i++)
            {
                TextrueProcessedFeedback feedback = _textureAgents[i].ResizeToMultipleOf4();
                if (feedback != null)
                {
                    feedbacks.Add(feedback);
                }
            }
            return feedbacks;
        }
    }
}