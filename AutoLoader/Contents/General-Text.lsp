;;by 天华AI研究中心
;;天华效率工具平台-通用工具-文字表格分类相关工具

;Match Text Content :文字内容刷：将文字内容改成源文字内容
(defun c:THMTC (/ txt1 txt2 ent)
    (princ "\n文字内容刷\n") 
    (setq ent (car (entsel "\n选择源文字：")))
    (setq txt1 (entget ent))
    (setq txt1 (cdr (assoc 1 txt1)))
    (while t
        (setq ent (car (entsel "\n选择目标文字：")))
        (setq ent (entget ent))
        (setq typ (cdr (assoc 0 enx)))
        (if (= "TCH_TEXT" typ)
            (progn
                (prompt "\n暂不支持天正文字对象，请炸成CAD文字后使用。")
            )
            (progn
                (setq ent 
                    (subst (cons 1 txt1) (assoc 1 ent) ent)
                )
                (entmod ent)
            )
        )
    );while
);defun
