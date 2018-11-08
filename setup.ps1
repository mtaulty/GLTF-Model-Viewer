Get-ChildItem ".\UnityGLTF\UnityGLTF\Assets\*.cs" -Recurse | ForEach-Object -Process {
    (Get-Content $_) -Replace 'namespace UnityGLTF', 'namespace KhronosUnityGLTF' | Set-Content $_
}

Get-ChildItem ".\UnityGLTF\UnityGLTF\Assets\*.cs" -Recurse | ForEach-Object -Process {
    (Get-Content $_) -Replace 'using UnityGLTF', 'using KhronosUnityGLTF' | Set-Content $_
}

Get-ChildItem ".\UnityGLTF\UnityGLTF\Assets\*.cs" -Recurse | ForEach-Object -Process {
    (Get-Content $_) -Replace 'namespace AssetGenerator', 'namespace KhronosAssetGenerator' | Set-Content $_
}

Get-ChildItem ".\UnityGLTF\UnityGLTF\Assets\*.cs" -Recurse | ForEach-Object -Process {
    (Get-Content $_) -Replace 'using AssetGenerator', 'using KhronosAssetGenerator' | Set-Content $_
}