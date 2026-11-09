using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fonctions utilitaires pour créer des mécaniques de puzzles.
/// </summary>
public static class PuzzleHelpers
{
    /// <summary>
    /// Spécifie l'axe ou les axes de rotation à utiliser.
    /// </summary>
    public enum RotationAxis { X, Y, Z, All }
    
    /// <summary>
    /// Vérifie si une liste d'objets est alignée sur une ligne droite avec une tolérance donnée.
    /// Utilise l'algorithme de distance perpendiculaire point-ligne.
    /// </summary>
    /// <param name="objects">La liste des objets à vérifier</param>
    /// <param name="tolerance">La distance maximale autorisée par rapport à la ligne (en unités Unity)</param>
    /// <returns>True si tous les objets sont alignés dans la tolérance</returns>
    /// <exception cref="Exception">Si la liste est vide</exception>
    public static bool AreAligned(List<Transform> objects, float tolerance)
    {
        switch (objects.Count)
        {
            case 0:
                throw new Exception("List vide");
            case <= 2:
                return true;
        }

        bool aligned = true;
        
        Vector3 firstPoint = objects[0].position;
        Vector3 ligneReference = (objects[1].position - firstPoint).normalized;
        
        for (int i = 2; i < objects.Count; i++)
        {
            Vector3 vectorFirstObjAndTarget = objects[i].position - firstPoint;
            Vector3 projection = Vector3.Project(vectorFirstObjAndTarget, ligneReference);

            if ((vectorFirstObjAndTarget - projection).magnitude < tolerance) continue;
            
            aligned = false;
            break;
        }

        return aligned;
    }

    /// <summary>
    /// Vérifie si une séquence d'entrée correspond exactement à la solution attendue.
    /// Utile pour les puzzles de code, mélodie, ou ordre d'activation.
    /// </summary>
    /// <param name="input">La séquence entrée par le joueur</param>
    /// <param name="solution">La séquence correcte attendue</param>
    /// <returns>True si les séquences sont identiques</returns>
    public static bool IsSequenceCorrect<T>(List<T> input, List<T> solution)
    {
        if (input.Count != solution.Count) return false;

        for (int i = 0; i < input.Count; i++) if (!input[i].Equals(solution[i])) return false;

        return true;
    }

    /// <summary>
    /// Arrondit une rotation au multiple le plus proche d'un angle donné.
    /// </summary>
    /// <param name="current">La rotation actuelle</param>
    /// <param name="snapAngle">L'incrément d'angle pour le snap (ex: 90° pour snapper tous les 90°)</param>
    /// <param name="rotationAxis">L'axe ou les axes à snapper</param>
    /// <returns>Une nouvelle rotation snappée</returns>
    public static Quaternion SnapRotation(Quaternion current, float snapAngle, RotationAxis rotationAxis)
    {
        Vector3 eulerAngleStart = current.eulerAngles;

        Vector3 snappedEuler = rotationAxis switch
        {
            RotationAxis.X => new Vector3(Snap(eulerAngleStart.x), eulerAngleStart.y, eulerAngleStart.z),
            RotationAxis.Y => new Vector3(eulerAngleStart.x, Snap(eulerAngleStart.y), eulerAngleStart.z),
            RotationAxis.Z => new Vector3(eulerAngleStart.x, eulerAngleStart.y, Snap(eulerAngleStart.z)),
            RotationAxis.All => new Vector3(Snap(eulerAngleStart.x), Snap(eulerAngleStart.y), Snap(eulerAngleStart.z)),
            _ => throw new ArgumentOutOfRangeException(nameof(rotationAxis), rotationAxis, null)
        };

        return Quaternion.Euler(snappedEuler);

        float Snap(float angle)
        {
            return Mathf.Round(angle / snapAngle) * snapAngle;
        }
    }
    
    /// <summary>
    /// Fait tourner un Transform d'un certain angle sur l'axe spécifié.
    /// </summary>
    /// <param name="target">Le Transform à faire tourner</param>
    /// <param name="amount">L'angle de rotation en degrés</param>
    /// <param name="axis">L'axe de rotation</param>
    public static void Rotate(this Transform target, float amount, RotationAxis axis)
    {
        Vector3 euler = target.rotation.eulerAngles;

        target.rotation = axis switch
        {
            RotationAxis.X => Quaternion.Euler(euler.x + amount, euler.y, euler.z),
            RotationAxis.Y => Quaternion.Euler(euler.x, euler.y + amount, euler.z),
            RotationAxis.Z => Quaternion.Euler(euler.x, euler.y, euler.z + amount),
            RotationAxis.All => Quaternion.Euler(euler.x + amount, euler.y + amount, euler.z + amount),
            _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
        };
    }

    /// <summary>
    /// Fait tourner un Transform d'un certain angle puis arrondit la rotation au snap le plus proche.
    /// Utile pour créer des objets rotables par incréments fixes (ex: blocs Portal, statues).
    /// </summary>
    /// <param name="target">Le Transform à faire tourner</param>
    /// <param name="amount">L'angle de rotation en degrés</param>
    /// <param name="snapAngle">L'incrément d'angle pour le snap</param>
    /// <param name="axis">L'axe de rotation</param>
    public static void RotateAndSnap(this Transform target, float amount, float snapAngle, RotationAxis axis)
    {
        target.Rotate(amount, axis);
        target.rotation = SnapRotation(target.rotation, snapAngle, axis);
    }
}