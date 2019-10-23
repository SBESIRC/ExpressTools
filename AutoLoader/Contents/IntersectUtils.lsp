;; http://lee-mac.com/intersectionfunctions.html

;; Intersections  -  Lee Mac
;; Returns a list of all points of intersection between two objects
;; for the given intersection mode.
;; ob1,ob2 - [vla] VLA-Objects
;;     mod - [int] acextendoption enum of intersectwith method

(defun LM:intersections ( ob1 ob2 mod / lst rtn )
    (if (and (vlax-method-applicable-p ob1 'intersectwith)
             (vlax-method-applicable-p ob2 'intersectwith)
             (setq lst (vlax-invoke ob1 'intersectwith ob2 mod))
        )
        (repeat (/ (length lst) 3)
            (setq rtn (cons (list (car lst) (cadr lst) (caddr lst)) rtn)
                  lst (cdddr lst)
            )
        )
    )
    (reverse rtn)
)

;; Intersections in Set  -  Lee Mac
;; Returns a list of all points of intersection between all objects in a supplied selection set.
;; sel - [sel] Selection Set

(defun LM:intersectionsinset ( sel / id1 id2 ob1 ob2 rtn )
    (repeat (setq id1 (sslength sel))
        (setq ob1 (vlax-ename->vla-object (ssname sel (setq id1 (1- id1)))))
        (repeat (setq id2 id1)
            (setq ob2 (vlax-ename->vla-object (ssname sel (setq id2 (1- id2))))
                  rtn (cons (LM:intersections ob1 ob2 acextendnone) rtn)
            )
        )
    )
    (apply 'append (reverse rtn))
)

;; Intersections in Object List  -  Lee Mac
;; Returns a list of all points of intersection between all objects in a list of VLA-Objects.
;; lst - [lst] List of VLA-Objects

(defun LM:intersectionsinobjlist ( lst / ob1 rtn )
    (while (setq ob1 (car lst))
        (foreach ob2 (setq lst (cdr lst))
            (setq rtn (cons (LM:intersections ob1 ob2 acextendnone) rtn))
        )
    )
    (apply 'append (reverse rtn))
)

;; Intersections Between Sets  -  Lee Mac
;; Returns a list of all points of intersection between objects in two selection sets.
;; ss1,ss2 - [sel] Selection sets

(defun LM:intersectionsbetweensets ( ss1 ss2 / id1 id2 ob1 ob2 rtn )
    (repeat (setq id1 (sslength ss1))
        (setq ob1 (vlax-ename->vla-object (ssname ss1 (setq id1 (1- id1)))))
        (repeat (setq id2 (sslength ss2))
            (setq ob2 (vlax-ename->vla-object (ssname ss2 (setq id2 (1- id2))))
                  rtn (cons (LM:intersections ob1 ob2 acextendnone) rtn)
            )
        )
    )
    (apply 'append (reverse rtn))
)

;; Intersections Between Object Lists  -  Lee Mac
;; Returns a list of all points of intersection between objects in two lists of VLA-Objects.
;; ol1,ol2 - [lst] Lists of VLA-Objects

(defun LM:intersectionsbetweenobjlists ( ol1 ol2 / rtn )
    (foreach ob1 ol1
        (foreach ob2 ol2
            (setq rtn (cons (LM:intersections ob1 ob2 acextendnone) rtn))
        )
    )
    (apply 'append (reverse rtn))
)