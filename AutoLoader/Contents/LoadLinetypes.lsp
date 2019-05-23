;; Load Linetypes  -  Lee Mac
;; Attempts to load a list of linetypes from any .lin files found in the support path.
;; Excludes known metric & imperial definition files based on the value of MEASUREMENT
;; lts - [lst] List of linetypes to load
;; rdf - [bol] If T, linetypes will be redefined from file if already loaded
;; Returns: [bol] T if all linetypes are loaded successfully, else nil

(defun LM:loadlinetypes ( lts rdf / lst ltc rtn val var )
    (cond
        (   (not (or rdf (vl-some '(lambda ( typ ) (not (tblsearch "ltype" typ))) lts))))
        (   (if (zerop (getvar 'measurement))
                (setq lst (mapcar 'strcase '("acadiso.lin" "iso.lin")))  ;; Known metric .lin files
                (setq lst (mapcar 'strcase '("acad.lin" "default.lin"))) ;; Known imperial .lin files
            )
            (setq ltc  (vla-get-linetypes (vla-get-activedocument (vlax-get-acad-object)))
                  lst  (vl-remove-if '(lambda ( x ) (member (strcase x) lst))
                           (apply 'append
                               (mapcar '(lambda ( dir ) (vl-directory-files dir "*.lin" 1))
                                   (vl-remove "" (LM:str->lst (getenv "ACAD") ";"))
                               )
                           )
                       )
            )
            (if rdf
                (progn
                    (setq var '(cmdecho expert)
                          val  (mapcar 'getvar var)
                    )
                    (mapcar 'setvar var '(0 5))
                )
            )
            (setq rtn
                (apply 'and
                    (mapcar
                       '(lambda ( typ )
                            (cond
                                (   (not (tblsearch "ltype" typ))
                                    (vl-some
                                       '(lambda ( lin )
                                            (vl-catch-all-apply 'vla-load (list ltc typ lin))
                                            (tblsearch "ltype" typ)
                                        )
                                        lst
                                    )
                                )
                                (   rdf
                                    (vl-some
                                       '(lambda ( lin )
                                            (and (LM:ltdefined-p typ lin)
                                                 (vl-cmdf "_.-linetype" "_L" typ lin "")
                                                 (tblsearch "ltype" typ)
                                            )
                                        )
                                        lst
                                    )
                                )
                                (   t   )
                            )
                        )
                        lts
                    )
                )
            )
            (mapcar 'setvar var val)
            rtn
        )
    )
)

;; Linetype Defined-p  -  Lee Mac
;; Returns T if the linetype is defined in the specified .lin file
;; ltp - [str] Linetype name
;; lin - [str] Filename of linetype definition file (.lin)

(defun LM:ltdefined-p ( ltp lin / str rtn )
    (if
        (and
            (setq lin (findfile lin))
            (setq lin (open lin "r"))
        )
        (progn
            (setq ltp (strcat "`*" (strcase ltp) "`,*"))
            (while
                (and (setq str (read-line lin))
                     (not (setq rtn (wcmatch (strcase str) ltp)))
                )
            )
            (close lin)
            rtn
        )
    )
)

;; String to List  -  Lee Mac
;; Separates a string using a given delimiter
;; str - [str] String to process
;; del - [str] Delimiter by which to separate the string
;; Returns: [lst] List of strings

(defun LM:str->lst ( str del / pos )
    (if (setq pos (vl-string-search del str))
        (cons (substr str 1 pos) (LM:str->lst (substr str (+ pos 1 (strlen del))) del))
        (list str)
    )
)

(vl-load-com) (princ)