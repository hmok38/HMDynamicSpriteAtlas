using System.Collections;
using System.Collections.Generic;
using HMDynamicSpriteAtlas;
using UnityEngine;

public class Test : MonoBehaviour
{
    private DynamicAtlasMgr _atlasMgr;

    public SpriteRenderer[] SpriteRenderers;

    // Start is called before the first frame update
    void Start()
    {
        _atlasMgr = new DynamicAtlasMgr();

        //
        // var sp = SpriteRenderers[0].sprite;
        // var text = _atlasMgr.GetSpriteTexture(sp);
        //
        // var spNew = Sprite.Create(text, new Rect(0, 0, text.width, text.height), new Vector2(0.5f, 0.5f));
        // SpriteRenderers[0].sprite = spNew;
        // return;

        string[] names = new string[SpriteRenderers.Length];
        Sprite[] sprites = new Sprite[SpriteRenderers.Length];
        for (int i = 0; i < SpriteRenderers.Length; i++)
        {
            sprites[i] = SpriteRenderers[i].sprite;
            names[i] = i.ToString();
        }

        _atlasMgr.RegSprites("Test", names, sprites);

        StartCoroutine("DelayTest");
    }


    private IEnumerator DelayTest()
    {
        yield return new WaitForSeconds(3);

        for (int i = 0; i < SpriteRenderers.Length; i++)
        {
            SpriteRenderers[i].sprite = _atlasMgr.GetSprite(i.ToString());
        }

        // yield return new WaitForSeconds(3);
        // _atlasMgr.DestroyDynamicTexture("Test");
    }

    // Update is called once per frame
    void Update()
    {
    }
}