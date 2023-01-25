using UnityEngine;

namespace FunkyCode.Rendering.Day
{
    public class Sorted
    {
        static public void Draw(Pass pass)
        {
            for(int id = 0; id < pass.sortList.Count; id++)
            {
                var sortObject = pass.sortList.List[id];
                var lightObject = sortObject.LightObject;

                if (lightObject is DayLightCollider2D dayCollider)
                {
                    if (!dayCollider.InAnyCamera()) 
                        continue;
                
                    if (pass.drawShadows)
                    {
                        if (dayCollider.mainShape.shadowType == DayLightCollider2D.ShadowType.Collider2D || dayCollider.mainShape.shadowType == DayLightCollider2D.ShadowType.SpritePhysicsShape)
                        {
                            Lighting2D.Materials.shadow.GetDayCPUShadow().SetPass (0);

                            GL.Begin(GL.TRIANGLES);
                            Shadow.Draw(dayCollider, pass.offset);  
                            GL.End(); 
                        }

                        SpriteRendererShadow.Begin(pass.offset);
                        SpriteRendererShadow.DrawOffset(dayCollider);

                        SpriteRendererShadow.End();
                    }

                    if (pass.drawMask)
                    {
                        SpriteRenderer2D.Draw(dayCollider, pass.offset);
                    }
                }
                else if (lightObject is DayLightTilemapCollider2D tilemapCollider)
                {
                    if (pass.drawShadows)
                    {
                        if (!tilemapCollider.ShadowsDisabled())
                        {
                            Lighting2D.Materials.shadow.GetDayCPUShadow().SetPass(0);

                            GL.Begin(GL.TRIANGLES);
                            Shadow.DrawTilemap(tilemapCollider, pass.offset, pass.camera);            
                            GL.End(); 
                        }
                    }
                    
                    if (pass.drawMask)
                    {
                        if (!tilemapCollider.MasksDisabled())
                        {
                            SpriteRenderer2D.DrawTilemap(tilemapCollider, pass.offset);
                        }
                    }
                }
            }
        }
    }
}
