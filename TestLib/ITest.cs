/////////////////////////////////////////////////////////////////////
// ITest.cs - define interfaces for test drivers and obj factory   //
//                                                                 //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * ITest.cs - define interfaces for test drivers and obj factory
 *
 * Required files:
 * ---------------
 * - ITest.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 16 Oct 2016
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadingTests
{
  public interface ITest
  {
    bool test();
  }
}
