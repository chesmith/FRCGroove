using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models.FRCv2
{
    public class PlayoffBracket
    {
        public Dictionary<string, int> brackets;

        /*
<table id="double-elim-bracket-table">
    <tbody><tr class="gap-row"></tr>

    <!-- Upper Bracket -->
    <tr>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 1</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class="winner"> 1</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/3847">3847</a>-<wbr><a href="/team/5414">5414</a>-<wbr><a href="/team/9054">9054</a>
                </span>
              </td>
              
                <td class="redScore winner">1</td>
              
            </tr>
          

          
            <tr>
              
                <td class=""> 8</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/8598">8598</a>-<wbr><a href="/team/8625">8625</a>-<wbr><a href="/team/3802">3802</a>
                </span>
              </td>
              
                <td class="blueScore ">0</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td rowspan="4" class="">
        <div class="merger inner"></div>
      </td>
    </tr>

    <tr>
      <td></td>
      <td class="dash"></td>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 7</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class="winner"> 1</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/3847">3847</a>-<wbr><a href="/team/5414">5414</a>-<wbr><a href="/team/9054">9054</a>
                </span>
              </td>
              
                <td class="redScore winner">1</td>
              
            </tr>
          

          
            <tr>
              
                <td class=""> 4</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/3679">3679</a>-<wbr><a href="/team/3561">3561</a>-<wbr><a href="/team/8827">8827</a>
                </span>
              </td>
              
                <td class="blueScore ">0</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td colspan="3"></td>
      <td rowspan="8" colspan="3">
        <div class="merger inner"></div>
      </td>
    </tr>

    <tr>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 2</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class="winner"> 4</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/3679">3679</a>-<wbr><a href="/team/3561">3561</a>-<wbr><a href="/team/8827">8827</a>
                </span>
              </td>
              
                <td class="redScore winner">1</td>
              
            </tr>
          

          
            <tr>
              
                <td class=""> 5</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/9121">9121</a>-<wbr><a href="/team/6369">6369</a>-<wbr><a href="/team/5503">5503</a>
                </span>
              </td>
              
                <td class="blueScore ">0</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td colspan="16"></td>
    </tr>

    <tr>
      <td colspan="4"></td>
      <td class="dash" colspan="2"></td>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 11</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class="winner"> 1</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/3847">3847</a>-<wbr><a href="/team/5414">5414</a>-<wbr><a href="/team/9054">9054</a>
                </span>
              </td>
              
                <td class="redScore winner">1</td>
              
            </tr>
          

          
            <tr>
              
                <td class=""> 2</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/5892">5892</a>-<wbr><a href="/team/9418">9418</a>-<wbr><a href="/team/8114">8114</a>
                </span>
              </td>
              
                <td class="blueScore ">0</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td colspan="9"></td>
      <td class="" colspan="3">
        <div class="top inner"></div>
      </td>
      <td class="" rowspan="9">
        <div class="merger inner"></div>
      </td>
    </tr>

    <tr>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 3</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class="winner"> 2</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/5892">5892</a>-<wbr><a href="/team/9418">9418</a>-<wbr><a href="/team/8114">8114</a>
                </span>
              </td>
              
                <td class="redScore winner">1</td>
              
            </tr>
          

          
            <tr>
              
                <td class=""> 7</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/2583">2583</a>-<wbr><a href="/team/9140">9140</a>-<wbr><a href="/team/5726">5726</a>
                </span>
              </td>
              
                <td class="blueScore ">0</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td rowspan="4">
        <div class="merger inner"></div>
      </td>
    </tr>

    <tr>
      <td></td>
      <td class="dash"></td>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 8</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class="winner"> 2</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/5892">5892</a>-<wbr><a href="/team/9418">9418</a>-<wbr><a href="/team/8114">8114</a>
                </span>
              </td>
              
                <td class="redScore winner">1</td>
              
            </tr>
          

          
            <tr>
              
                <td class=""> 6</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/8507">8507</a>-<wbr><a href="/team/8749">8749</a>-<wbr><a href="/team/8879">8879</a>
                </span>
              </td>
              
                <td class="blueScore ">0</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td colspan="3"></td>
      <td colspan="10"></td>
      <td class="dash" colspan="1"></td>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Finals</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class="winner"> 1</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/3847">3847</a>-<wbr><a href="/team/5414">5414</a>-<wbr><a href="/team/9054">9054</a>
                </span>
              </td>
              
                <td class="redScore winner">2</td>
              
            </tr>
          

          
            <tr>
              
                <td class=""> 5</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/9121">9121</a>-<wbr><a href="/team/6369">6369</a>-<wbr><a href="/team/5503">5503</a>
                </span>
              </td>
              
                <td class="blueScore ">0</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 4</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class=""> 3</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/418">418</a>-<wbr><a href="/team/8019">8019</a>-<wbr><a href="/team/6357">6357</a>
                </span>
              </td>
              
                <td class="redScore ">0</td>
              
            </tr>
          

          
            <tr>
              
                <td class="winner"> 6</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/8507">8507</a>-<wbr><a href="/team/8749">8749</a>-<wbr><a href="/team/8879">8879</a>
                </span>
              </td>
              
                <td class="blueScore winner">1</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td></td>
    </tr>

    <!--    Lower Bracket -->
    <tr>
      <td colspan="16"></td>
    </tr>

    <tr>
      <td colspan="11"></td>
      <td class="dash"></td>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 13</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class=""> 2</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/5892">5892</a>-<wbr><a href="/team/9418">9418</a>-<wbr><a href="/team/8114">8114</a>
                </span>
              </td>
              
                <td class="redScore ">0</td>
              
            </tr>
          

          
            <tr>
              
                <td class="winner"> 5</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/9121">9121</a>-<wbr><a href="/team/6369">6369</a>-<wbr><a href="/team/5503">5503</a>
                </span>
              </td>
              
                <td class="blueScore winner">1</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td colspan="5"></td>
      <td class="dash" colspan="1"></td>

      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 10</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class=""> 6</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/8507">8507</a>-<wbr><a href="/team/8749">8749</a>-<wbr><a href="/team/8879">8879</a>
                </span>
              </td>
              
                <td class="redScore ">0</td>
              
            </tr>
          

          
            <tr>
              
                <td class="winner"> 5</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/9121">9121</a>-<wbr><a href="/team/6369">6369</a>-<wbr><a href="/team/5503">5503</a>
                </span>
              </td>
              
                <td class="blueScore winner">1</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
      <td colspan="3"></td>
      <td rowspan="3">
        <div class="snake inner"></div>
      </td>
    </tr>

    <tr>
      <td colspan="4"></td>
      <td rowspan="2">
        <div class="snake inner"></div>
      </td>
      <td colspan="1"></td>
      <td rowspan="4" class="">
        <div class="merger inner"></div>
      </td>
    </tr>

    <tr>
      <td colspan="3"></td>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 5</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class=""> 8</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/8598">8598</a>-<wbr><a href="/team/8625">8625</a>-<wbr><a href="/team/3802">3802</a>
                </span>
              </td>
              
                <td class="redScore ">0</td>
              
            </tr>
          

          
            <tr>
              
                <td class="winner"> 5</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/9121">9121</a>-<wbr><a href="/team/6369">6369</a>-<wbr><a href="/team/5503">5503</a>
                </span>
              </td>
              
                <td class="blueScore winner">1</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
      <td colspan="3"></td>
      <td class="dash"></td>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 12</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class="winner"> 5</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/9121">9121</a>-<wbr><a href="/team/6369">6369</a>-<wbr><a href="/team/5503">5503</a>
                </span>
              </td>
              
                <td class="redScore winner">1</td>
              
            </tr>
          

          
            <tr>
              
                <td class=""> 7</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/2583">2583</a>-<wbr><a href="/team/9140">9140</a>-<wbr><a href="/team/5726">5726</a>
                </span>
              </td>
              
                <td class="blueScore ">0</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td colspan="1"></td>
    </tr>

    <tr>
      <td colspan="5"></td>
      <td class="dash" colspan="1"></td>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 9</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class=""> 4</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/3679">3679</a>-<wbr><a href="/team/3561">3561</a>-<wbr><a href="/team/8827">8827</a>
                </span>
              </td>
              
                <td class="redScore ">0</td>
              
            </tr>
          

          
            <tr>
              
                <td class="winner"> 7</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/2583">2583</a>-<wbr><a href="/team/9140">9140</a>-<wbr><a href="/team/5726">5726</a>
                </span>
              </td>
              
                <td class="blueScore winner">1</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td colspan="4"></td>
      <td rowspan="2" class="path">
        <div class="snake inner"></div>
      </td>
    </tr>

    <tr>
      <td colspan="3"></td>
      <td rowspan="2" class="match">
        
  <div class="match-table-wrapper">
    <div style="position: relative;">
      <span class="match-label">Match 6</span>
      <table class="match-table">
        
          
            <tbody><tr>
              
                <td class="winner"> 7</td>
              
              <td>
                <span class="alliance-name winner">
                  
                  <wbr><a href="/team/2583">2583</a>-<wbr><a href="/team/9140">9140</a>-<wbr><a href="/team/5726">5726</a>
                </span>
              </td>
              
                <td class="redScore winner">1</td>
              
            </tr>
          

          
            <tr>
              
                <td class=""> 3</td>
              
              <td>
                <span class="alliance-name ">
                  
                  <wbr><a href="/team/418">418</a>-<wbr><a href="/team/8019">8019</a>-<wbr><a href="/team/6357">6357</a>
                </span>
              </td>
              
                <td class="blueScore ">0</td>
              
            </tr>
          
        
      </tbody></table>
    </div>
  </div>

      </td>
    </tr>

    <tr>
      <td colspan="1"></td>
    </tr>

    <tr class="gap-row"></tr>

  </tbody></table>         */

        public PlayoffBracket(List<Alliance> alliances, List<FRCMatch> matches)
        {
            if (alliances != null && matches != null)
            {
                foreach (Alliance alliance in alliances)
                {
                    alliance.LoadTeams();
                }

                brackets = new Dictionary<string, int>();

                List<FRCMatch> sf1 = matches.Where(m => m.title == "Semifinal 1-1").ToList();
                List<FRCMatch> sf2 = matches.Where(m => m.title == "Semifinal 2-1").ToList();
                List<FRCMatch> f = matches.Where(m => m.title == "Final 1").ToList();

                brackets["qf1-red"] = 1;
                brackets["qf1-blue"] = 8;
                brackets["qf2-red"] = 4;
                brackets["qf2-blue"] = 5;
                brackets["qf3-red"] = 2;
                brackets["qf3-blue"] = 7;
                brackets["qf4-red"] = 3;
                brackets["qf4-blue"] = 6;

                brackets["sf1-red"] = alliances.Where(a => sf1.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Red")))).Select(a => a.number).FirstOrDefault();
                brackets["sf1-blue"] = alliances.Where(a => sf1.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Blue")))).Select(a => a.number).FirstOrDefault();
                brackets["sf2-red"] = alliances.Where(a => sf2.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Red")))).Select(a => a.number).FirstOrDefault();
                brackets["sf2-blue"] = alliances.Where(a => sf2.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Blue")))).Select(a => a.number).FirstOrDefault();

                brackets["f-red"] = alliances.Where(a => f.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Red")))).Select(a => a.number).FirstOrDefault();
                brackets["f-blue"] = alliances.Where(a => f.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Blue")))).Select(a => a.number).FirstOrDefault();
            }
        }
    }
}
