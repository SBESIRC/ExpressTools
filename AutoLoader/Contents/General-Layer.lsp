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

(defun TH:procLayerColor ( index / data )
    (setq rules (strcat *pluginContentPath* "\\Standards\\Layer\\Process.csv"))
    (if (setq data (LM:readcsv rules))
        (progn
            (foreach line data
                (setq name      (nth 2 line))
                (setq color     (nth index line))
                (command "._-layer" "color" color name "")
            )
            (layerstate-save "建筑底图" 255 nil)
            (princ)
        )
    )
)

(defun c:THALC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if omeasurement (setvar 'measurement omeasurement))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    ; we want to use imperial .lin files in a metric drawing
    (setq omeasurement (getvar 'measurement))
    (setvar 'measurement 0) ;Imperial
   
    (setq group "天华建筑标准图层")
    ; Import text styles and dimension styles from the standard drawing
    (Steal (strcat *pluginContentPath* "\\Standards\\Style\\THArchitecture.dwg")
        '(
            (
                "Text Styles"
                ("TH-STYLE*")
            )
            (
                "Dimension Styles"
                ("TH-DIM*")
            )
         )
    )
    ; Load line types from the standard line definition file
    (LM:loadlinetypes '("HIDDEN" "CENTER") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Architecture.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    ; Line Type Scale factor globally
    (setvar 'ltscale 1000)
    
    (setvar 'cmdecho oecho)
    (setvar 'measurement omeasurement)
    (princ)
);defun

(defun c:THSLC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if omeasurement (setvar 'measurement omeasurement))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    ; we want to use imperial .lin files in a metric drawing
    (setq omeasurement (getvar 'measurement))
    (setvar 'measurement 0) ;Imperial
    
    (setq group "天华结构标准图层")
    ; Import text styles and dimension styles from the standard drawing
    (Steal (strcat *pluginContentPath* "\\Standards\\Style\\THStructure.dwg")
        '(
            (
                "Text Styles"
                ("TH-STYLE*")
            )
            (
                "Dimension Styles"
                ("TSSD_*_*")
            )
         )
    )
    ; Load line types from the standard line definition file
    (LM:loadlinetypes '("CENTER" "DASHED" "DASHED2") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Structure.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    ; Line Type Scale factor globally
    (setvar 'ltscale 500)
    
    (setvar 'cmdecho oecho)
    (setvar 'measurement omeasurement)
    (princ)
);defun

(defun c:THMLC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if omeasurement (setvar 'measurement omeasurement))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    ; we want to use imperial .lin files in a metric drawing
    (setq omeasurement (getvar 'measurement))
    (setvar 'measurement 0) ;Imperial
    
    (setq group "天华暖通标准图层")
    ; Import text styles and dimension styles from the standard drawing
    (Steal (strcat *pluginContentPath* "\\Standards\\Style\\THHVAC.dwg")
        '(
            (
                "Text Styles"
                ("TH-STYLE*")
            )
            (
                "Dimension Styles"
                ("TH-DIM*")
            )
         )
    )
    ; Load line types from the standard line definition file
    (LM:loadlinetypes '("HIDDEN" "CENTER2" "DASHED2") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\HVAC.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    ; Line Type Scale factor globally
    (setvar 'ltscale 1000)
    
    (setvar 'cmdecho oecho)
    (setvar 'measurement omeasurement)
    (princ)
);defun

(defun c:THELC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if omeasurement (setvar 'measurement omeasurement))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    ; we want to use imperial .lin files in a metric drawing
    (setq omeasurement (getvar 'measurement))
    (setvar 'measurement 0) ;Imperial
    
    (setq group "天华电气标准图层")
    ; Import text styles and dimension styles from the standard drawing
    (Steal (strcat *pluginContentPath* "\\Standards\\Style\\THElectrical.dwg")
        '(
            (
                "Text Styles"
                ("TH-STYLE*")
            )
            (
                "Dimension Styles"
                ("E-DIMA*" "TH-DIM*")
            )
         )
    )
    ; Load line types from the standard line definition file
    (LM:loadlinetypes '("HIDDEN" "HIDDEN2" "DASHDOT" "DASHDOT2" "DIVIDE2" "E-GND" "E-THU" "PHANTOM" "PHANTOM2" "BORDER2" "CENTER" "DIVIDE" "DIVIDE2" "DASHED") nil)
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Electrical.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    ; Line Type Scale factor globally
    (setvar 'ltscale 500)
    
    (setvar 'cmdecho oecho)
    (setvar 'measurement omeasurement)
    (princ)
);defun

(defun c:THPLC ( / *error* oecho group )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if omeasurement (setvar 'measurement omeasurement))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    ; we want to use imperial .lin files in a metric drawing
    (setq omeasurement (getvar 'measurement))
    (setvar 'measurement 0) ;Imperial

    (setq group "天华给排水标准图层")
    ; Import text styles from the standard drawing
    (Steal (strcat *pluginContentPath* "\\Standards\\DWG\\THPlumbing.dwg")
        '(
            (
                "Text Styles"
                ("TH-STYLE*" "W-text")
            )
            (
                "Dimension Styles"
                ("TH-DIM*-W" "w-DN*")
            )
         )
    )
    ; Load line types from the standard line definition file
    (LM:loadlinetypes '(
                            "CENTER2"
                            "BORDER"
                            "DASHED"
                            "DASHDOT"
                            "J生活给水"
                            "J1中区生活给水"
                            "J2高区生活给水"
                            "J0屋顶水箱进水管"
                            "R生活热水管" 
                            "R1中区生活热水管" 
                            "R2高区生活热水管" 
                            "RH热水回水管" 
                            "RH1中区热水回水管" 
                            "RH2高区热水回水管"
                            "RM热媒水管"
                            "RMH热媒回水管"
                            "Y雨水管"
                            "HY虹吸雨水管"
                            "X消火栓消防管"
                            "X1中区消火栓消防管"
                            "X2高区消火栓消防管"
                            "ZP自动喷淋管"
                            "ZP1中区自动喷淋管"
                            "ZP2高区自动喷淋管"
                            "SP水炮给水管线"
                            "Q气体灭火管线"
                            "F废水管线"
                            "W污水管线"
                            "YW压力污水管线"
                            "YF压力废水管线"
                            "YY压力雨水管线"
                            "ZM水幕管"
                            "ZY雨淋管"
                            "ZW水喷雾管"
                            "ZXW细水雾管"
                            "G燃气管"
                            "G燃气放散管"
                            "Z蒸汽管"
                            "N蒸汽凝结水管"
                            "ZJ中水给水管"
                            "ZJ1中区中水给水管"
                            "ZJ2中区中水给水管"
                            "P总图污水管线"
                            "E总图强电管线"
                            "T总图弱电管线"
                        ) 
                        nil
        )
    (TH:DeleteLayerGroupFilter group)
    (TH:loadCSV (strcat *pluginContentPath* "\\Standards\\Layer\\Plumbing.csv"))
    (TH:RenameLayerGroupFilter *defaultParentGroup* group)
    (TH:ShowLayerManagerPalette group)
    (TH:ActivateLayerGroupFilter group)
    
    ; Line Type Scale factor globally
    (setvar 'ltscale 500)
    
    (setvar 'cmdecho oecho)
    (setvar 'measurement omeasurement)
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

; 处理建筑结构底图（暖通）
(defun c:THLPM ( / *error* oecho )
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
    (TH:procLayerColor 13)
    (command ".undo" "E")
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

; 处理建筑结构底图（电气）
(defun c:THLPE ( / *error* oecho )
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
    (TH:procLayerColor 14)
    (command ".undo" "E")
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

; 处理建筑结构底图（给排水）
(defun c:THLPP ( / *error* oecho )
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
    (TH:procLayerColor 15)
    (command ".undo" "E")
    
    (setvar 'cmdecho oecho)
    (princ)
);defun

；解锁所有图层
(defun c:THUKA ( / *error* oecho )
    (defun *error* ( msg )
        (if oecho (setvar 'cmdecho oecho))
        (if (not (member msg '("Function cancelled" "quit / exit abort")))
            (princ (strcat "\nError: " msg))
         )
        (princ)
    )
    
    (setq oecho (getvar 'cmdecho))
    (setvar 'cmdecho 0)
    
    (command "._-layer" "unlock" "*" "")
    
    (setvar 'cmdecho oecho)
    (princ)
);defun
