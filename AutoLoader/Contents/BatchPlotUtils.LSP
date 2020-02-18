;;by 天华AI研究中心

;**********获取系统文件夹路径*************
;; References
;;  https://forums.augi.com/showthread.php?154607-Folder-Browser-Dialog-for-LISP
;;  https://forums.autodesk.com/t5/visual-lisp-autolisp-and-general/why-vla-get-hwnd-ping-tim-willey-and-tony-tanzillo/td-p/1481901
;; Examples:
;; (lgm_GETPATH "Select a Folder" nil)
;; (lgm_GETPATH nil nil)
;; (lgm_GETPATH nil "C:\\users")
(defun lgm_GETPATH (msg path / *error* acApp oShell oFolder)
  (defun *error* (msg)
    (if    oShell (vlax-release-object oShell))
    (cond ((not msg))                            ; Normal exit
      ((member msg '("Function cancelled" "quit / exit abort")))    ; <esc> or (quit)
      ((princ (strcat "\n** Error: " msg " ** ")))            ; Fatal error, display it
    )
    (if    oFolder path nil)
  )

  (and
    (or
      (and (= 'STR (type path)) (findfile path))
      (setq path (+ 1 64 256))
    )
    (or (= 'STR (type msg)) (setq msg "Select a Folder to Search"))
    (setq acApp (vlax-get-acad-object))
    (setq oShell (vla-GetInterfaceObject acApp "Shell.Application"))
    (setq oFolder
     (vlax-invoke
       oShell
       'BrowseForFolder
       (vla-get-hwnd acApp)
       msg
       0
       path
     )
    )
    (if oFolder
      (progn
        (setq folderobject (vlax-get-property oFolder 'Self))
        (setq path (vlax-get-property folderobject 'Path))
        (if (/= (substr path (strlen path)) "\\")
          (setq path (strcat path "\\"))
        )
      )
    )
  )
  (*error* nil)
)

;;-----------------------------------------------------------------------------------------------;;

(vl-load-com)
(princ)

;;-----------------------------------------------------------------------------------------------;;
;;                                          End of File                                          ;;
;;-----------------------------------------------------------------------------------------------;;