using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace HMDynamicSpriteAtlas
{
    public class DynamicAtlasMgr
    {
        /// <summary>
        /// 图集的最大尺寸
        /// </summary>
        public int MaxWidthHeight = 2048;

        private Dictionary<string, TextureAtlasInfo> _texture2DMap = new Dictionary<string, TextureAtlasInfo>();


        /// <summary>
        /// (性能消耗巨大) 注册sprite,后面会使用这个名字或者id获取到已经在动态图集中的sprite
        /// 注册过后的sprite的纹理,原则上可以从内存中卸载了,因为内存中已经复制了一份
        /// </summary>
        /// <param name="textureId"></param>
        /// <param name="nameOrIds"></param>
        /// <param name="sprites"></param>
        public void RegSprites(string textureId, string[] nameOrIds, Sprite[] sprites)
        {
            if (_texture2DMap.ContainsKey(textureId))
            {
                Debug.LogError(
                    $"已经有了名叫 {textureId} 的动态纹理集,其中有 {_texture2DMap[textureId].SpriteAtlasInfoMap.Count} 个sprite");
                return;
            }

            Texture2D[] tempTexture2Ds = new Texture2D[sprites.Length];
            for (int i = 0; i < nameOrIds.Length; i++)
            {
                if (sprites[i] == null)
                {
                    Debug.LogError(
                        $"传入的sprite有空对象,序号为{i}  名字或id为{nameOrIds[i]}");
                    return;
                }

                if (string.IsNullOrEmpty(nameOrIds[i]) || string.IsNullOrWhiteSpace(nameOrIds[i]))
                {
                    Debug.LogError(
                        $"传入的sprite名字为空,序号为{i} sprites名字为 {sprites[i].name}");
                    return;
                }

                if (!sprites[i].texture.isReadable)
                {
                    Debug.LogError(
                        $"动态图集需要纹理设置为Read/Write,请将 {sprites[i].name} 的纹理 {sprites[i].texture.name} 设置为Read/Write");
                    return;
                }

                tempTexture2Ds[i] = GetSpriteTexture(sprites[i]);
            }

            Texture2D newT = new Texture2D(1, 1);
            var rects = newT.PackTextures(tempTexture2Ds, 4, MaxWidthHeight);

            for (int i = 0; i < tempTexture2Ds.Length; i++)
            {
                Object.Destroy(tempTexture2Ds[i]); //销毁临时的texture
            }

            var tInfo = new TextureAtlasInfo()
            {
                TextureId = textureId,
                SpriteAtlasInfoMap = new Dictionary<string, Sprite>(sprites.Length),
                MyTexture2D = newT,
            };
            for (int i = 0; i < nameOrIds.Length; i++)
            {
                var spriteRect = new Rect(rects[i].x * newT.width, rects[i].y * newT.height,
                    rects[i].width * newT.width, rects[i].height * newT.height);

                var spritePivot = new Vector2(sprites[i].pivot.x / sprites[i].rect.width,
                    sprites[i].pivot.y / sprites[i].rect.height);
                tInfo.SpriteAtlasInfoMap.Add(nameOrIds[i],
                    Sprite.Create(newT, spriteRect,
                        spritePivot,
                        sprites[i].pixelsPerUnit));
            }

            _texture2DMap.Add(textureId, tInfo);
        }

        /// <summary>
        /// 获取动态图集中的sprite,指定了textureId的就会在此texture中找
        /// 没有的话会遍历
        /// </summary>
        /// <param name="nameOrId"></param>
        /// <param name="textureId"></param>
        /// <returns></returns>
        public Sprite GetSprite(string nameOrId, string textureId = "")
        {
            //有传textureId就直接找
            if (!string.IsNullOrEmpty(textureId))
            {
                if (!_texture2DMap.TryGetValue(textureId, out var textureAtlasInfo)) return null;
                return textureAtlasInfo.SpriteAtlasInfoMap.TryGetValue(nameOrId, out var sprite) ? sprite : null;
            }

            //没有就遍历找
            var map = _texture2DMap.Values.FirstOrDefault(x => x.SpriteAtlasInfoMap.ContainsKey(nameOrId));
            if (map == null) return null;
            return map.SpriteAtlasInfoMap.TryGetValue(nameOrId, out var sprite2) ? sprite2 : null;

            return null;
        }

        /// <summary>
        /// 销毁动态纹理-会从内存中移除
        /// </summary>
        public void DestroyDynamicTexture(string textureId)
        {
            if (_texture2DMap.TryGetValue(textureId, out var textureAtlasInfo))
            {
                if (textureAtlasInfo != null && textureAtlasInfo.MyTexture2D != null)
                {
                    Object.Destroy(textureAtlasInfo.MyTexture2D);
                }

                _texture2DMap.Remove(textureId);
            }
        }

        public Texture2D GetSpriteTexture(Sprite sprite)
        {
            // 获取Sprite的像素区域
            Rect rect = sprite.rect;
            Texture2D newTexture = new Texture2D((int)rect.width, (int)rect.height,TextureFormat.RGBA32,sprite.texture.mipmapCount,true)
                {
                    filterMode = sprite.texture.filterMode,
                    wrapMode = sprite.texture.wrapMode
                };
            // 获取原始纹理数据
            Texture2D sourceTexture = sprite.texture;
            Color[] pixels = sourceTexture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);

            // 将像素数据设置到新纹理中
            newTexture.SetPixels(pixels);
            newTexture.Apply();
            // newTexture.filterMode = sprite.texture.filterMode;
            // newTexture.wrapMode = sprite.texture.wrapMode;
            return newTexture;
        }


        class TextureAtlasInfo
        {
            public string TextureId;
            public Dictionary<string, Sprite> SpriteAtlasInfoMap;
            public Texture2D MyTexture2D;
        }
    }
}