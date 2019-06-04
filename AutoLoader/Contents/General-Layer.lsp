(defun TH:loadCSV ( file / data )
    (if (setq data (LM:readcsv file))
        (progn
            (regapp "AcAecLayerStandard")
            (foreach line data
                (setq name      (nth 2 line))
                (setq color     (atoi (nth 6 line)))
                (setq ltype     (nth 7 line))
                (setq plot      (= (nth 11 line) "T"))
                (setq desc      (nth 12 line))
                (MakeLayer name color ltype -3 plot 0 desc)
            )
            ; layer group filters
            (setq groups nil)
            (foreach line data
                (setq groups (cons (nth 0 line) groups))    
            )
            ; remove duplicated group
            (LM:Unique groups)
            ; create parent layer group filter
            (setq root "全部")
            (setq parent "天华标准图层")
            (TH:MakeLayerGroupFilter root parent nil)
            ; create sub layer group filter
            (foreach group groups
                (TH:MakeLayerGroupFilter parent group nil)
            )
            ; add layers to the layer group filters
            (foreach line data
                (TH:AddToLayerGroup (nth 2 line) (nth 0 line))
            )
            (princ)
        )
    )
)

(setq *defaultParentGroup* "天华标准图层")
(setq *pluginPath* (strcat (getenv "PROGRAMFILES") "\\Autodesk\\ApplicationPlugins\\ThCADPlugin.bundle"))
(setq *pluginContentPath* (strcat *pluginPath* "\\Contents"))

(defun c:THALC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    (setq group "天华建筑标准图层")
    (LM:loadlinetypes '("HIDDEN" "CENTER") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Architecture.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THSLC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    (setq group "天华结构标准图层")
    (LM:loadlinetypes '("CENTER" "DASHED" "DASHED2") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Structure.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THMLC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    (setq group "天华暖通标准图层")
    (LM:loadlinetypes '("HIDDEN" "CENTER2" "DASHED2") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\HVAC.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THELC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    (setq group "天华电气标准图层")
    (LM:loadlinetypes '("HIDDEN" "HIDDEN2" "DASHDOT" "DASHDOT2" "DIVIDE2" "E-GND" "E-THU" "PHANTOM" "PHANTOM2" "BORDER2" "CENTER" "DIVIDE" "DIVIDE2" "DASHED") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Electrical.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THPLC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)

    (setq group "天华给排水标准图层")
    ; Import text styles from the standard drawing
    (Steal (strcat *pluginContentPath* "\\Standards\\DWG\\Plumbing.dwg")
        '(
            (
                "Text Styles"
                ("TH-STYLE*")
            )
         )
    )
    ; Load line types from the standard line definition file
    (LM:loadlinetypes '(
                            "CENTER2"
                            "BORDER"
                            "DASHED"
                            "DASHDOT"
                            "TH_PL01-100_J" 
                            "TH_PL01-110_J1" 
                            "TH_PL01-120_J2" 
                            "TH_PL02-100_R" 
                            "TH_PL02-110_R1" 
                            "TH_PL02-120_R2"
                            "TH_PL04-100_RMH"
                            "TH_PL05-100_RM"
                            "TH_PL06-100_ZJ"
                            "TH_PL06-110_ZJ1"
                            "TH_PL06-120_ZJ2"
                            "TH_PL07-100_RH"
                            "TH_PL07-110_RH1"
                            "TH_PL07-120_RH2"
                            "TH_PL09-100_XH"
                            "TH_PL09-110_XH1"
                            "TH_PL09-120_XH2"
                            "TH_PL10-100_ZP"
                            "TH_PL10-110_ZP1"
                            "TH_PL10-120_ZP2"
                            "TH_PL11-100_SP"
                            "TH_PL12-100_Q"
                            "TH_PL13-100_F"
                            "TH_PL14-100_YF"
                            "TH_PL16-100_Y"
                            "TH_PL17-100_YY"
                            "TH_PL18-100_W"
                            "TH_PL19-100_YW"
                            "TH_PL21-100_T"
                            "TH_PL20-100_HY"
                            "TH_PL18-110_W"
                        ) 
                        nil
        )
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Plumbing.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THMLK ( / *error* oecho )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    (setvar 'clayer  "0")
    
    (command ".undo" "BE")
    (command "._-layer" "lock" "D-*" "")
    (command "._-layer" "lock" "H-*" "")
    (command ".undo" "E") 
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THMUK ( / *error* oecho )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    (setvar 'clayer  "0")
    
    (command ".undo" "BE")
    (command "._-layer" "lock" "*" "")
    (command "._-layer" "unlock" "0" "")
    (command "._-layer" "unlock" "D-*" "")
    (command "._-layer" "unlock" "H-*" "")
    (command ".undo" "E") 
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THMON ( / *error* oecho )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    (setvar 'clayer  "0")
    
    (command ".undo" "BE")
    (command "._-layer" "on" "D-*" "")
    (command "._-layer" "on" "H-*" "")
    (command ".undo" "E")
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

(defun c:THMOF ( / *error* oecho )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    (setvar 'clayer  "0")
    
    (command ".undo" "BE")
    (command "._-layer" "off" "D-*" "")
    (command "._-layer" "off" "H-*" "")
    (command ".undo" "E")
    
    (setvar 'cmdecho oecho)
    (princ)
);defun
