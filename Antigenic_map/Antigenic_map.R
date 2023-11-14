library(tidyverse)
library(gridExtra)
library(Racmacs)
library(ggacmap)
library(data.table)
library(ggrepel)
library(ggforce)
library(grid)

set.seed(12345678)

excel_file <- "20230911_ID50-shenzhou.xlsx"
all_sheet <- readxl::excel_sheets(excel_file)

IC50_data <- sapply(all_sheet,function(x){
  readxl::read_xlsx(excel_file,sheet = x) %>% 
    rename(lineage=1) 
}) %>% enframe() %>% mutate(group=name) %>% 
  mutate(value=map2(group,value,~{
    grp <- .x
    rename_with(.y,~paste0(grp,"_",.x),.cols=-1)
  }))

IC50_data <- IC50_data %>% group_by(group) %>% nest() %>% 
  mutate(value=map(data,~(.$value %>% reduce(left_join,by="lineage")))) %>% 
  mutate(name=group) %>% ungroup() 

dat_index <- c(2:5)
a <- reduce(IC50_data$value[dat_index],left_join,by="lineage")

b <- data.matrix(a[,-1])
rownames(b) <- a$lineage

b <- apply(b,2,function(x){
  x <- ifelse(is.na(x),40,x)
  ifelse(x<=10,"<10",x)
})


map <- acmap(titer_table = b)

map1 <- optimizeMap(
  map                     = map,
  number_of_dimensions    = 2,
  number_of_optimizations = 10000,
  minimum_column_basis    = "none"
)
xx <- Racmacs:::mapPoints(map = map1,optimization_number = 1)$coords

points_coor <- xx %>% as.data.frame() %>%  rownames_to_column("Var") %>% rename(x_axis=V1,y_axis=V2) %>% 
  mutate(type=rep(c("lineage",IC50_data$group[dat_index]),
                  times=c(IC50_data$value[[1]] %>% nrow(),sapply(IC50_data$value[dat_index],ncol)-1)))

raw <- do.call(rbind,lapply(1:length(IC50_data$value),function(i){
  data.table(
    group=IC50_data$group[[i]],
    IC50_data$value[[i]] %>%
      melt()
  )
}))

test <- points_coor
test <- test %>%
  merge(
    raw %>% 
      group_by(type=group,Var=variable) %>%
      summarise(value1=mean(value,na.rm = TRUE)),all.x=T
  ) %>%
  merge(
    raw %>%
      group_by(type='lineage',Var=lineage) %>%
      summarise(value2=mean(value,na.rm = TRUE)),all.x=T
  ) %>%
  mutate(value=ifelse(is.na(value1),value2,value1)) %>%
  filter(!is.na(value))


new_name <- setNames(IC50_data$group,IC50_data$group)


new_name <- c(new_name,"lineage"="lineage")

new_name <- new_name %>% as.data.frame() %>% 
  mutate(color=palette()[length(new_name):1]) %>% 
  mutate(pch_type=c(0,15,0,15,0,1))

test <- test %>% mutate(type=new_name[type,"."]) %>% left_join(new_name,by=c("type"="."))

test <- test %>% mutate(tmp=x_axis) %>% mutate(x_axis=y_axis) %>% mutate(y_axis=tmp)

g_plots <- ggplot() + 
  geom_point(aes(x=x_axis,y=y_axis,colour=color,size=25,shape=pch_type),
             alpha=0.2,
             data=test %>% filter(type!='lineage')) + scale_shape_identity(guide = "legend") +
  scale_color_identity(guide = "legend")+
  geom_point(aes(x=x_axis,y=y_axis,fill=Var,size=25),
             shape=21,colour='black',alpha=0.8,
             data=test %>% filter(type=='lineage')) +
  geom_label_repel(aes(x=x_axis,y=y_axis,label=Var),
                   data=test %>% filter(type=='lineage',Var!="HK.3"),size=12.5,
                   label.size = NA,fill=alpha("white",0.1),
                   nudge_x = ifelse((test %>% filter(type=='lineage'))$Var=="BE.1.1.1",-0.05,0),max.time = 5) +
  geom_label_repel(aes(x=x_axis,y=y_axis,label=Var),
                   data=test %>% filter(Var%in%c('BA.2.86')),size=12.5,label.size = NA,
                   fill=alpha("white",0.1),min.segment.length = 0,box.padding = 0.5,
                   nudge_x = c(0.25),nudge_y = c(-0.4),segment.size=1)+
  geom_mark_ellipse(data = test %>% 
                      filter(!Var%in%c("WT","B.1.351","B.1.617.2","BA.1","BA.2","BA.5","BF.7","BQ.1.1","CH.1.1"),
                             type=="lineage"),
                    expand = unit(0.3,"cm"),
                    aes(color="red",x=x_axis,y=y_axis,label="XBB.*"),
                    label.fill = alpha("white",0),con.colour = alpha("white",0),label.fontsize = 40,
                    label.fontface = "plain",label.buffer = unit(0,"cm")) + 
  scale_y_continuous(breaks=scales::breaks_width(1)) + scale_x_continuous(breaks=scales::breaks_width(1))+ 
  theme_bw() + theme(panel.grid=element_blank())+
  theme(legend.position='none',text=element_text(size=0),
        panel.grid.major.y = element_line(color = "grey",size = 0.25,linetype = 1),
        panel.grid.major.x = element_line(color = "grey",size = 0.25,linetype = 1)) + coord_fixed()


pdf(file = "20230912_antigenic_map.pdf",width = 14,height = 14)
g_plots
dev.off()

