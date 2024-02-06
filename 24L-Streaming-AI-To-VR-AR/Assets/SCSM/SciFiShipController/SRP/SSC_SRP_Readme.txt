Scriptable Render Pipeline assets in SSC require Unity 2020.3.25 LTS or newer.
IMPORTANT: Do not import these packages without URP/HDRP installed, else you will get many errors. 

1. Import the entire asset (e.g. Sci-Fi Ship Controller) into a project setup for URP or HDRP.
2. If using HDRP, from package manager, import High Definition Render Pipeline 10.7.0 (U2020.3.25 LTS+)
3. If using URP, from the package manager, import Universal Render Pipeline 10.7.0 or newer (U2020.3.25 LTS+)
4. From the Unity Editor double-click** on either the SSC_URP_[version] or SSC_HDRP_[version] package within this folder
5. TechDemo has its own packages, SSC_TechDemo_URP_[version] or SSC_TechDemo_HDRP_[version]. These require U2020.3.25+ or newer.
6. TechDemo3 has its own packages, SSC_TechDemo3_URP_[version] or SSC_TechDemo3_HDRP_[version]. These require U2020.3.25+ or newer.

** If double-click does not work, from the Unity Editor menu, Assets/Import Package/Custom Package... navigate to the folder within your project where the package is located to import the package.

NOTE: Celestials (stars) do not work with HDRP as Unity do not support depth-only cameras in these Render Pipelines.
Therefore, in this release stars will not be visible in the Asteriods demo unless the built-in or URP 10.7.0+ pipelines are used.

For HDRP in 2020.x install the HRDP 10.7.0 and SSC_TechDemo_HDRP_10.7.0 packages. You may need to lower the Sun Lux from say 120000 to 80000 and reduce the Sky Exposure from say 16 to 14 in some of the demo scenes. Adjust values to suit your tastes.