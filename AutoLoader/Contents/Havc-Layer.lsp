; 暖通图层模块快速切换

; 关闭所有图层
(defun TH:HideAll()            
    (command "-layer" "oFF" "H-BASE" "")    
    (command "-layer" "oFF" "H-BASE-DIMS" "")
    (command "-layer" "oFF" "H-BUSH" "")
    (command "-layer" "oFF" "H-BUSH-DIMS" "")
    (command "-layer" "oFF" "H-DAPP-AAPP" "")
    (command "-layer" "oFF" "H-DAPP-ADAMP" "")
    (command "-layer" "oFF" "H-DAPP-DAMP" "")
    (command "-layer" "oFF" "H-DAPP-DAPP" "")
    (command "-layer" "oFF" "H-DAPP-DDAMP" "")
    (command "-layer" "oFF" "H-DAPP-DGRIL" "")
    (command "-layer" "oFF" "H-DAPP-EDAMP" "")
    (command "-layer" "oFF" "H-DAPP-GRIL" "")
    (command "-layer" "oFF" "H-DIMS" "")
    (command "-layer" "oFF" "H-DIMS-DUAL" "")
    (command "-layer" "oFF" "H-DIMS-DUCT" "")
    (command "-layer" "oFF" "H-DIMS-EQUP" "")
    (command "-layer" "oFF" "H-DIMS-FANS" "")
    (command "-layer" "oFF" "H-DIMS-VRV" "")
    (command "-layer" "oFF" "H-DUAL-FBOX" "")
    (command "-layer" "oFF" "H-DUCT" "")
    (command "-layer" "oFF" "H-DUCT-ACON" "")
    (command "-layer" "oFF" "H-DUCT-APPE" "")
    (command "-layer" "oFF" "H-DUCT-DUAL" "")
    (command "-layer" "oFF" "H-DUCT-VENT" "")
    (command "-layer" "oFF" "H-EQUP" "")
    (command "-layer" "oFF" "H-EQUP-ELSE" "")
    (command "-layer" "oFF" "H-EQUP-FAN" "")
    (command "-layer" "oFF" "H-EQUP-FANS" "")
    (command "-layer" "oFF" "H-EQUP-FBOX" "")
    (command "-layer" "oFF" "H-EQUP-FC" "")
    (command "-layer" "oFF" "H-EQUP-GRIL" "")
    (command "-layer" "oFF" "H-EQUP-VRV" "")
    (command "-layer" "oFF" "H-HOBU-DIMS" "")
    (command "-layer" "oFF" "h-hole-dims" "")
    (command "-layer" "oFF" "H-EQUP-AHU" "")
    (command "-layer" "oFF" "H-AI" "")
    (command "-layer" "oFF" "H-AI-CTRL" "")
    (command "-layer" "oFF" "H-AI-DIMS" "")
    (command "-layer" "oFF" "H-AI-INST" "")
    (command "-layer" "oFF" "H-AI-RECT" "")
    (command "-layer" "lock" "H-AI" "")
    (command "-layer" "lock" "H-AI-CTRL" "")
    (command "-layer" "lock" "H-AI-DIMS" "")
    (command "-layer" "lock" "H-AI-INST" "")
    (command "-layer" "lock" "H-AI-RECT" "")
    (command "-layer" "oFF" "H-BUSH" "")
    (command "-layer" "oFF" "H-BUSH-DIMS" "")
    (command "-layer" "oFF" "H-EQUP" "")
    (command "-layer" "oFF" "H-EQUP-AHU" "")
    (command "-layer" "oFF" "H-EQUP-CHI" "")
    (command "-layer" "oFF" "H-EQUP-CT" "")
    (command "-layer" "oFF" "H-EQUP-ELSE" "")
    (command "-layer" "oFF" "H-EQUP-FC" "")
    (command "-layer" "oFF" "H-EQUP-GRIL" "")
    (command "-layer" "oFF" "H-EQUP-HEAT" "")
    (command "-layer" "oFF" "H-EQUP-HP" "")
    (command "-layer" "oFF" "H-EQUP-PUMP" "")
    (command "-layer" "oFF" "H-EQUP-SW" "")
    (command "-layer" "oFF" "H-PAPP-DIMS" "")
    (command "-layer" "oFF" "H-PAPP-ELSE" "")
    (command "-layer" "oFF" "h-papp-evalv" "")
    (command "-layer" "oFF" "h-papp-valv" "")
    (command "-layer" "oFF" "H-PIPE" "")
    (command "-layer" "oFF" "H-PIPE-APPE" "")
    (command "-layer" "oFF" "H-PIPE-C" "")
    (command "-layer" "oFF" "H-PIPE-CHR" "")
    (command "-layer" "oFF" "H-PIPE-CHS" "")
    (command "-layer" "oFF" "H-PIPE-CR" "")
    (command "-layer" "oFF" "H-PIPE-CS" "")
    (command "-layer" "oFF" "H-PIPE-CTR" "")
    (command "-layer" "oFF" "H-PIPE-CTS" "")
    (command "-layer" "oFF" "H-PIPE-DIMS" "")
    (command "-layer" "oFF" "H-PIPE-HR" "")
    (command "-layer" "oFF" "H-PIPE-HS" "")
    (command "-layer" "oFF" "H-PIPE-R" "")
    (command "-layer" "oFF" "H-VALV-DIMS" "")
    (command "-layer" "oFF" "H-EQUP-VRV" "")
    (command "-layer" "oFF" "H-DIMS-EQUP" "")
    (command "-layer" "oFF" "H-DIMS-VRV" "")
    (command "-layer" "oFF" "H-DAPP-DAPP" "")
    (command "-layer" "oFF" "H-DAPP-DDAMP" "")
    (command "-layer" "oFF" "H-DAPP-DGRIL" "")
    (command "-layer" "oFF" "H-DIMS-FAPP" "")
    (command "-layer" "oFF" "H-DAPP-FDAMP" "")
    (command "-layer" "oFF" "H-DAPP-FGRIL" "")
    (command "-layer" "oFF" "H-DIMS-DUAL" "")
    (command "-layer" "oFF" "H-DIMS-FIRE" "")
    (command "-layer" "oFF" "H-DUCT-FIRE" "")
    (command "-layer" "oFF" "H-FIRE-FBOX" "")
    (command "-layer" "oFF" "H-DUCT-DUAL" "")
)

; 切换当前图层到"0"图层
(defun TH:SwitchToZero()
    (command "-layer" "t" "0" "")
    (command "-layer" "u" "0" "")
    (command "-layer" "on" "0" "")
    (setvar 'clayer  "0")
)

; 进入通风模式
(defun c:THTF(  / *error* )            
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)

    (command ".undo" "BE")
    (TH:SwitchToZero)
    (TH:HideAll)
    (command "-layer" "on" "H-BASE" "")    
    (command "-layer" "on" "H-BASE-DIMS" "")
    (command "-layer" "on" "H-BUSH" "")
    (command "-layer" "on" "H-BUSH-DIMS" "")
    (command "-layer" "on" "H-DAPP-AAPP" "")
    (command "-layer" "on" "H-DAPP-ADAMP" "")
    (command "-layer" "on" "H-DAPP-DAMP" "")
    (command "-layer" "on" "H-DAPP-DAPP" "")
    (command "-layer" "on" "H-DAPP-DDAMP" "")
    (command "-layer" "on" "H-DAPP-DGRIL" "")
    (command "-layer" "on" "H-DAPP-EDAMP" "")
    (command "-layer" "on" "H-DAPP-GRIL" "")
    (command "-layer" "on" "H-DIMS" "")
    (command "-layer" "on" "H-DIMS-DUAL" "")
    (command "-layer" "on" "H-DIMS-DUCT" "")
    (command "-layer" "on" "H-DIMS-EQUP" "")
    (command "-layer" "on" "H-DIMS-FANS" "")
    (command "-layer" "on" "H-DIMS-VRV" "")
    (command "-layer" "on" "H-DUAL-FBOX" "")
    (command "-layer" "on" "H-DUCT" "")
    (command "-layer" "on" "H-DUCT-ACON" "")
    (command "-layer" "on" "H-DUCT-APPE" "")
    (command "-layer" "on" "H-DUCT-DUAL" "")
    (command "-layer" "on" "H-DUCT-VENT" "")
    (command "-layer" "on" "H-EQUP" "")
    (command "-layer" "on" "H-EQUP-ELSE" "")
    (command "-layer" "on" "H-EQUP-FAN" "")
    (command "-layer" "on" "H-EQUP-FANS" "")
    (command "-layer" "on" "H-EQUP-FBOX" "")
    (command "-layer" "on" "H-EQUP-FC" "")
    (command "-layer" "on" "H-EQUP-GRIL" "")
    (command "-layer" "on" "H-EQUP-VRV" "")
    (command "-layer" "on" "H-HOBU-DIMS" "")
    (command "-layer" "on" "h-hole-dims" "")
    (command "-layer" "on" "H-EQUP-AHU" "")
    (command ".undo" "E") 

    (setvar 'cmdecho oecho)
    (princ)
)

; 进入水管模式
(defun c:THSG( / *error* )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)

    (command ".undo" "BE")
    (TH:SwitchToZero)
    (TH:HideAll)
    (command "-layer" "on" "H-BUSH" "")
    (command "-layer" "on" "H-BUSH-DIMS" "")
    (command "-layer" "on" "H-EQUP" "")
    (command "-layer" "on" "H-EQUP-AHU" "")
    (command "-layer" "on" "H-EQUP-CHI" "")
    (command "-layer" "on" "H-EQUP-CT" "")
    (command "-layer" "on" "H-EQUP-ELSE" "")
    (command "-layer" "on" "H-EQUP-FC" "")
    (command "-layer" "on" "H-EQUP-GRIL" "")
    (command "-layer" "on" "H-EQUP-HEAT" "")
    (command "-layer" "on" "H-EQUP-HP" "")
    (command "-layer" "on" "H-EQUP-PUMP" "")
    (command "-layer" "on" "H-EQUP-SW" "")
    (command "-layer" "on" "H-PAPP-DIMS" "")
    (command "-layer" "on" "H-PAPP-ELSE" "")
    (command "-layer" "on" "h-papp-evalv" "")
    (command "-layer" "on" "h-papp-valv" "")
    (command "-layer" "on" "H-PIPE" "")
    (command "-layer" "on" "H-PIPE-APPE" "")
    (command "-layer" "on" "H-PIPE-C" "")
    (command "-layer" "on" "H-PIPE-CHR" "")
    (command "-layer" "on" "H-PIPE-CHS" "")
    (command "-layer" "on" "H-PIPE-CR" "")
    (command "-layer" "on" "H-PIPE-CS" "")
    (command "-layer" "on" "H-PIPE-CTR" "")
    (command "-layer" "on" "H-PIPE-CTS" "")
    (command "-layer" "on" "H-PIPE-DIMS" "")
    (command "-layer" "on" "H-PIPE-HR" "")
    (command "-layer" "on" "H-PIPE-HS" "")
    (command "-layer" "on" "H-PIPE-R" "")
    (command "-layer" "on" "H-VALV-DIMS" "")
    (command "-layer" "on" "H-EQUP-VRV" "")
    (command "-layer" "on" "H-DIMS-EQUP" "")
    (command "-layer" "on" "H-DIMS-VRV" "")
    (command ".undo" "E") 

    (setvar 'cmdecho oecho)
    (princ)
)

; 进入消防模式
(defun c:THXF( / *error* )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)

    (command ".undo" "BE")
    (TH:SwitchToZero)
    (TH:HideAll)
    (command "-layer" "on" "H-DAPP-DAPP" "")
    (command "-layer" "on" "H-DAPP-DDAMP" "")
    (command "-layer" "on" "H-DAPP-DGRIL" "")
    (command "-layer" "on" "H-DIMS-FAPP" "")
    (command "-layer" "on" "H-DAPP-FDAMP" "")
    (command "-layer" "on" "H-DAPP-FGRIL" "")
    (command "-layer" "on" "H-DIMS-DUAL" "")
    (command "-layer" "on" "H-DIMS-FIRE" "")
    (command "-layer" "on" "H-DUCT-FIRE" "")
    (command "-layer" "on" "H-FIRE-FBOX" "")
    (command "-layer" "on" "H-DUCT-DUAL" "")    
    (command ".undo" "E") 

    (setvar 'cmdecho oecho)
    (princ)
)