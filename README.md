# ExpTreeLib
A Class Library for building Forms with a folder navigation TreeView and form specific ListViews that can be tailored for your application and behave like Windows Explorer.

This project is from Jim Parsells, he published it in CodeProject.
For more details, please refer to below link.
The original license is CPOL 1.02.

Project link in CodeProject:
https://www.codeproject.com/Articles/422497/ExpTreeLib-Version-3-Explorer-like-Navigation-and

Original license link:
https://www.codeproject.com/info/cpol10.aspx

This is a very good user control to build your own Windows Explorer. But this proejct hasn't been maintained for several years, so I recompiled it, and did some improvments, added some functions. The good thing is that I create new user contol which name is ExpListLib for list view based on Jim Parsells code. This will help us to build our own Windows Explorer easier.

![image](https://github.com/kaifuzi/ExpTreeLib/blob/main/ExpListLib_Demo.png)

In fact, I'm not sure below crash issue is from ExpTreeLib or not: 
This control works very well in Win7, but in Win10, when I keep the application opening, it's crashed occasionally. I think it's because of memory overstack, but I haven't found out the reason. If you just open the application for a while, and then close, there is no problem.
  - With author Jim Parsells's help, this stack overflow bug is fixed!
