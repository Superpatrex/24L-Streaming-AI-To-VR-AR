Scriptable Render Pipeline assets in S3D require Unity 2020.3.25f1 or newer.
IMPORTANT: Do not import these packages without URP or HDRP installed, else you will get many errors.

1. Import the entire asset (e.g. Sticky3D Controller) into a project setup for URP, or HDRP.
2. If using HDRP, from package manager, import High Definition Render Pipeline 10.7.0 or newer (U2020.3.25f1 LTS+)
3. If using URP, from the package manager, import Universal Render Pipeline 10.7.0 or newer (U2020.3.25f1 LTS+)
4. From the Unity Editor double-click** on either the S3D_URP_[version] or S3D_HDRP_[version] package within this folder

** If double-click does not work, from the Unity Editor menu, Assets/Import Package/Custom Package... navigate to the folder within your project where the package is located to import the package.

NOTE: The GravityShooterDemo doesn't include the weapon camera in HDRP as Unity do not support depth-only cameras in these Render Pipelines.
Therefore, in this release the weapon will not be visible unless the built-in or URP 10.7.0+ pipelines are used.
